#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Compilation;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using AutoMapper;
using CustomerPortal.Client;
using CustomerPortal.Contract.Response;
using Infrastructure.EventSourcing;
using HoneyBadger;
using Infrastructure.Messaging;
using log4net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using CreateOrder = apcurium.MK.Booking.Api.Contract.Requests.CreateOrder;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (CreateOrderService));

        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly HoneyBadgerServiceClient _honeyBadgerServiceClient;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IPaymentService _paymentService;
        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly Resources.Resources _resources;

        public CreateOrderService(ICommandBus commandBus,
            IAccountDao accountDao,
            IServerSettings serverSettings,
            ReferenceDataService referenceDataService,
            IIBSServiceProvider ibsServiceProvider,
            IRuleCalculator ruleCalculator,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IAccountChargeDao accountChargeDao,
            ICreditCardDao creditCardDao,
            IOrderDao orderDao,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            HoneyBadgerServiceClient honeyBadgerServiceClient,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IPaymentService paymentService,
            IPayPalServiceFactory payPalServiceFactory,
            IOrderPaymentDao orderPaymentDao)
        {
            _accountChargeDao = accountChargeDao;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _ruleCalculator = ruleCalculator;
            _orderDao = orderDao;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _honeyBadgerServiceClient = honeyBadgerServiceClient;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _paymentService = paymentService;
            _payPalServiceFactory = payPalServiceFactory;
            _orderPaymentDao = orderPaymentDao;

            _resources = new Resources.Resources(_serverSettings);
        }

        public object Post(CreateOrder request)
        {
            Log.Info("Create order request : " + request.ToJson());

            // TODO: Find a better way to do this...
            var isFromWebApp = request.FromWebApp;

            if (!isFromWebApp)
            {
                ValidateAppVersion(request.ClientLanguageCode);
            }

            // Find market
            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.PickupAddress.Latitude, request.PickupAddress.Longitude);

            if (market.HasValue())
            {
                // Only pay in car charge type supported for orders outside home market
                request.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
            }
            else
            {
                // Ensure that the market is not an empty string
                market = null;
            }

            var isPrepaid = isFromWebApp
                && (request.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                    || request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id);

            BestAvailableCompany bestAvailableCompany;

            if (request.OrderCompanyKey.HasValue() || request.OrderFleetId.HasValue)
            {
                // For API user, it's possible to manually specify which company to dispatch to by using a fleet id
                bestAvailableCompany = FindSpecificCompany(market, request.OrderCompanyKey, request.OrderFleetId);
            }
            else
            {
                bestAvailableCompany = FindBestAvailableCompany(market, request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }

            if (market.HasValue() && bestAvailableCompany.CompanyKey == null)
            {
                // No companies available that are desserving this region for the company
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrder_NoCompanies", request.ClientLanguageCode));
            }

            UpdateVehicleTypeFromMarketData(request.Settings, bestAvailableCompany.CompanyKey);

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            account.IBSAccountId = CreateIbsAccountIfNeeded(account, bestAvailableCompany.CompanyKey);
            
            var isFutureBooking = request.PickupDate.HasValue;
            var pickupDate = request.PickupDate ?? GetCurrentOffsetedTime(bestAvailableCompany.CompanyKey);

            // User can still create future order, but we allow only one active Book now order.
            if (!isFutureBooking)
            {
                var pendingOrderId = GetPendingOrder();

                // We don't allow order creation if there's already an order scheduled
                if (!_serverSettings.ServerData.AllowSimultaneousAppOrders
                    && pendingOrderId != null
                    && !isFromWebApp)
                {
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_PendingOrder.ToString(), pendingOrderId.ToString());
                }
            }

            var rule = _ruleCalculator.GetActiveDisableFor(
                isFutureBooking,
                pickupDate,
                () =>
                    _ibsServiceProvider.StaticData(bestAvailableCompany.CompanyKey)
                        .GetZoneByCoordinate(
                            request.Settings.ProviderId,
                            request.PickupAddress.Latitude,
                            request.PickupAddress.Longitude),
                () => request.DropOffAddress != null
                    ? _ibsServiceProvider.StaticData(bestAvailableCompany.CompanyKey)
                        .GetZoneByCoordinate(
                            request.Settings.ProviderId,
                            request.DropOffAddress.Latitude,
                            request.DropOffAddress.Longitude)
                        : null,
                market);

            if (rule != null)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);
            }

            // We need to validate the rules of the roaming market.
            if(market.HasValue())
            {
                // External market, query company site directly to validate their rules
                var orderServiceClient = new RoamingValidationServiceClient(bestAvailableCompany.CompanyKey, _serverSettings.ServerData.Target);

                Log.Info(string.Format("Validating rules for company in external market... Target: {0}, Server: {1}", _serverSettings.ServerData.Target, orderServiceClient.Url));

                var validationResult = orderServiceClient.ValidateOrder(request, true);
                if (validationResult.HasError)
                {
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), validationResult.Message);
                }
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                throw new HttpError(ErrorCode.CreateOrder_SettingsRequired.ToString());
            }

            ReferenceData referenceData;

            if (market.HasValue())
            {
                referenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = bestAvailableCompany.CompanyKey });
            }
            else
            {
                referenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest());
            }

            request.PickupDate = pickupDate;

            request.Settings.Passengers = request.Settings.Passengers <= 0
                ? 1
                : request.Settings.Passengers;

            if (_serverSettings.ServerData.Direction.NeedAValidTarif
                && (!request.Estimate.Price.HasValue || request.Estimate.Price == 0))
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_NoFareEstimateAvailable.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_NoFareEstimateAvailable, request.ClientLanguageCode));
            }

            // IBS provider validation
            ValidateProvider(request, referenceData, market.HasValue());

            // Map the command to obtain a OrderId (web doesn't prepopulate it in the request)
            var orderCommand = Mapper.Map<Commands.CreateOrder>(request);

            // Promo code validation
            var promotionId = ValidatePromotion(request.PromoCode, request.Settings.ChargeTypeId, account.Id, orderCommand.OrderId, pickupDate, isFutureBooking, request.ClientLanguageCode);

            // Charge account validation
            var accountValidationResult = ValidateChargeAccountIfNecessary(request, orderCommand.OrderId, account, isFutureBooking, market, isFromWebApp);

            // if ChargeAccount uses payment with card on file, payment validation was already done
            if (!accountValidationResult.IsChargeAccountPaymentWithCardOnFile)
            {
                // Payment method validation
                ValidatePayment(request, orderCommand.OrderId, account, isFutureBooking, request.Estimate.Price, isPrepaid);
            }
            
            // Initialize PayPal if user is using PayPal web
            var paypalWebPaymentResponse = InitializePayPalCheckoutIfNecessary(account.Id, isPrepaid, orderCommand.OrderId, request);

            var chargeTypeIbs = string.Empty;
            var chargeTypeEmail = string.Empty;
            var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == request.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            chargeTypeKey = accountValidationResult.ChargeTypeKeyOverride ?? chargeTypeKey;

            if (chargeTypeKey != null)
            {
                // this must be localized with the priceformat to be localized in the language of the company
                // because it is sent to the driver
                chargeTypeIbs = _resources.Get(chargeTypeKey, _serverSettings.ServerData.PriceFormat);

                chargeTypeEmail = _resources.Get(chargeTypeKey, request.ClientLanguageCode);
            }

            // Get Vehicle Type from reference data
            var vehicleType = referenceData.VehiclesList
                .Where(x => x.Id == request.Settings.VehicleTypeId)
                .Select(x => x.Display)
                .FirstOrDefault();

            var ibsInformationNote = BuildNote(chargeTypeIbs, request.Note, request.PickupAddress.BuildingName, request.Settings.LargeBags);
            var fare = GetFare(request.Estimate);

            orderCommand.AccountId = account.Id;
            orderCommand.UserAgent = Request.UserAgent;
            orderCommand.ClientVersion = Request.Headers.Get("ClientVersion");
            orderCommand.IsChargeAccountPaymentWithCardOnFile = accountValidationResult.IsChargeAccountPaymentWithCardOnFile;
            orderCommand.CompanyKey = bestAvailableCompany.CompanyKey;
            orderCommand.CompanyName = bestAvailableCompany.CompanyName;
            orderCommand.Market = market;
            orderCommand.IsPrepaid = isPrepaid;
            orderCommand.Settings.ChargeType = chargeTypeIbs;
            orderCommand.Settings.VehicleType = vehicleType;
            orderCommand.IbsAccountId = account.IBSAccountId.Value;
            orderCommand.ReferenceDataCompanyList = referenceData.CompaniesList.ToArray();
            orderCommand.IbsInformationNote = ibsInformationNote;
            orderCommand.Fare = fare;
            orderCommand.Prompts = accountValidationResult.Prompts;
            orderCommand.PromptsLength = accountValidationResult.PromptsLength;
            orderCommand.PromotionId = promotionId;

            Debug.Assert(request.PickupDate != null, "request.PickupDate != null");

            _commandBus.Send(orderCommand);

            if (paypalWebPaymentResponse != null)
            {
                // Order prepaid by PayPal
                _commandBus.Send(new SaveTemporaryOrderCreationInfo
                {
                    OrderId = orderCommand.OrderId,
                    SerializedOrderCreationInfo = new TemporaryOrderCreationInfo
                    {
                        OrderId = orderCommand.OrderId,
                        Account = account,
                        Request = orderCommand,
                        ReferenceDataCompaniesList = referenceData.CompaniesList,
                        ChargeTypeIbs = chargeTypeIbs,
                        ChargeTypeEmail = chargeTypeEmail,
                        VehicleType = vehicleType,
                        Prompts = accountValidationResult.Prompts,
                        PromptsLength = accountValidationResult.PromptsLength,
                        BestAvailableCompany = bestAvailableCompany,
                        PromotionId = promotionId,
                    }.ToJson()
                });

                return paypalWebPaymentResponse;
            }

            return new OrderStatusDetail
            {
                OrderId = orderCommand.OrderId,
                Status = OrderStatus.Created,
                IBSStatusId = string.Empty,
                IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", orderCommand.ClientLanguageCode),
            };
        }

        public object Get(ExecuteWebPaymentAndProceedWithOrder request)
        {
            Log.Info("ExecuteWebPaymentAndProceedWithOrder request : " + request.ToJson());

            var temporaryInfo = _orderDao.GetTemporaryInfo(request.OrderId);
            TemporaryOrderCreationInfo orderInfo = null;

            try
            {
                orderInfo = JsonSerializer.DeserializeFromString<TemporaryOrderCreationInfo>(temporaryInfo.SerializedOrderCreationInfo);
            }
            catch (Exception ex)
            {
                Log.Error("Unable to deserialize TemporaryOrderCreationInfo", ex);
            }

            if (request.Cancel || orderInfo == null)
            {
                var clientLanguageCode = orderInfo == null
                    ? SupportedLanguages.en.ToString()
                    : orderInfo.Request.ClientLanguageCode;

                _commandBus.Send(new CancelOrderBecauseOfError
                {
                    OrderId = request.OrderId,
                    WasPrepaid = true,
                    ErrorDescription = _resources.Get("CannotCreateOrder_PrepaidPayPalPaymentCancelled", clientLanguageCode)
                });
            }
            else
            {
                // Execute PayPal payment
                var response = _payPalServiceFactory.GetInstance().ExecuteWebPayment(request.PayerId, request.PaymentId);

                if (response.IsSuccessful)
                {
                    var tipPercentage = orderInfo.Account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;
                    var tipAmount = FareHelper.CalculateTipAmount(orderInfo.Request.Fare.AmountInclTax, tipPercentage);

                    _commandBus.Send(new MarkPrepaidOrderAsSuccessful
                    {
                        OrderId = request.OrderId,
                        Amount = orderInfo.Request.Fare.AmountInclTax + tipAmount,
                        Meter = orderInfo.Request.Fare.AmountExclTax,
                        Tax = orderInfo.Request.Fare.TaxAmount,
                        Tip = tipAmount,
                        TransactionId = response.TransactionId,
                        Provider = PaymentProvider.PayPal,
                        Type = PaymentType.PayPal
                    });
                }
                else
                {
                    _commandBus.Send(new CancelOrderBecauseOfError
                    {
                        OrderId = request.OrderId,
                        WasPrepaid = true,
                        ErrorDescription = response.Message
                    });
                }
            }

            // Build url used to redirect the web client to the booking status view
            string baseUrl;

            if (_serverSettings.ServerData.BaseUrl.HasValue())
            {
                baseUrl = _serverSettings.ServerData.BaseUrl;
            }
            else
            {
                baseUrl = Request.AbsoluteUri
                    .Replace(Request.PathInfo, string.Empty)
                    .Replace(GetAppHost().Config.ServiceStackHandlerFactoryPath, string.Empty)
                    .Replace(Request.QueryString.ToString(), string.Empty)
                    .Replace("?", string.Empty);
            }    

            var redirectUrl = string.Format("{0}#status/{1}", baseUrl, request.OrderId);

            return new HttpResult
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers = { { HttpHeaders.Location, redirectUrl } }
            };
        }

        public object Post(SwitchOrderToNextDispatchCompanyRequest request)
        {
            Log.Info("Switching order to another IBS : " + request.ToJson());

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var order = _orderDao.FindById(request.OrderId);
            var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);

            if (orderStatusDetail.Status != OrderStatus.TimedOut)
            {
                // Only switch companies if order is timedout
                return orderStatusDetail;
            }

            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

            var newOrderRequest = new CreateOrder
            {
                PickupDate = GetCurrentOffsetedTime(request.NextDispatchCompanyKey),
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.DropOffAddress,
                Settings = new BookingSettings
                {
                    LargeBags = order.Settings.LargeBags,
                    Name = order.Settings.Name,
                    NumberOfTaxi = order.Settings.NumberOfTaxi,
                    Passengers = order.Settings.Passengers,
                    Phone = order.Settings.Phone,
                    ProviderId = null,

                    // Payment in app is not supported for now when we use another IBS
                    ChargeType = ChargeTypes.PaymentInCar.Display,
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,

                    // Reset vehicle type
                    VehicleType = null,
                    VehicleTypeId = null
                },
                Note = order.UserNote,
                ClientLanguageCode = account.Language
            };

            var fare = GetFare(new CreateOrder.RideEstimate {Price = order.EstimatedFare});
            var newReferenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = request.NextDispatchCompanyKey });

            // This must be localized with the priceformat to be localized in the language of the company
            // because it is sent to the driver
            var chargeTypeIbs = _resources.Get(ChargeTypes.PaymentInCar.Display, _serverSettings.ServerData.PriceFormat);
            var ibsInformationNote = BuildNote(chargeTypeIbs, order.UserNote, order.PickupAddress.BuildingName, newOrderRequest.Settings.LargeBags);

            var networkErrorMessage = string.Format(_resources.Get("Network_CannotCreateOrder", order.ClientLanguageCode), request.NextDispatchCompanyName);

            int ibsAccountId;
            try
            {
                // Recreate order on next dispatch company IBS
                ibsAccountId = CreateIbsAccountIfNeeded(account, request.NextDispatchCompanyKey);
            }
            catch (Exception ex)
            {
                Log.Error(networkErrorMessage, ex);
                throw new HttpError(HttpStatusCode.InternalServerError, networkErrorMessage);
            }

            ValidateProvider(newOrderRequest, newReferenceData, market.HasValue());

            var newOrderCommand = Mapper.Map<Commands.CreateOrder>(newOrderRequest);
            newOrderCommand.OrderId = request.OrderId;
            newOrderCommand.ReferenceDataCompanyList = newReferenceData.CompaniesList.ToArray();
            newOrderCommand.Market = market;
            newOrderCommand.CompanyKey = request.NextDispatchCompanyKey;
            newOrderCommand.CompanyName = request.NextDispatchCompanyName;
            newOrderCommand.Fare = fare;
            newOrderCommand.IbsInformationNote = ibsInformationNote;

            _commandBus.Send(new InitiateIbsOrderSwitch
            {
                NewIbsAccountId = ibsAccountId,
                NewOrderCommand = newOrderCommand
            });

            return new OrderStatusDetail
            {
                OrderId = request.OrderId,
                Status = OrderStatus.Created,
                CompanyKey = request.NextDispatchCompanyKey,
                CompanyName = request.NextDispatchCompanyName,
                NextDispatchCompanyKey = null,
                NextDispatchCompanyName = null,
                IBSStatusId = string.Empty,
                IBSStatusDescription = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", order.ClientLanguageCode), request.NextDispatchCompanyName),
            };
        }

        public object Post(IgnoreDispatchCompanySwitchRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            _commandBus.Send(new IgnoreDispatchCompanySwitch
            {
                OrderId = request.OrderId
            });

            return new HttpResult(HttpStatusCode.OK);
        }

        private ChargeAccountValidationResult ValidateChargeAccountIfNecessary(CreateOrder request, Guid orderId, AccountDetail account, bool isFutureBooking, string market, bool isFromWebApp)
        {
            string[] prompts = null;
            int?[] promptsLength = null;
            string chargeTypeOverride = null;
            var isChargeAccountPaymentWithCardOnFile = false;

            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.Account.Id)
            {
                var accountChargeDetail = _accountChargeDao.FindByAccountNumber(request.Settings.AccountNumber);

                if (accountChargeDetail.UseCardOnFileForPayment)
                {
                    if (market.HasValue())
                    {
                        // Card on file payment not supported when not in home market
                        throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrderChargeAccountNotSupportedInRoaming", request.ClientLanguageCode));
                    }

                    if (isFromWebApp)
                    {
                        // Charge account cannot support prepaid orders
                        throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrderChargeAccountNotSupportedOnWeb", request.ClientLanguageCode));
                    }

                    if (_paymentService.IsPayPal(account.Id))
                    {
                        chargeTypeOverride = ChargeTypes.PayPal.Display;
                        request.Settings.ChargeTypeId = ChargeTypes.PayPal.Id;
                    }
                    else
                    {
                        chargeTypeOverride = ChargeTypes.CardOnFile.Display;
                        request.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                    }
                    
                    isChargeAccountPaymentWithCardOnFile = true;
                }

                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.Settings.CustomerNumber, request.QuestionsAndAnswers, request.ClientLanguageCode);

                if (isChargeAccountPaymentWithCardOnFile)
                {
                    ValidatePayment(request, orderId, account, isFutureBooking, request.Estimate.Price, false);
                }

                prompts = request.QuestionsAndAnswers.Select(q => q.Answer).ToArray();
                promptsLength = request.QuestionsAndAnswers.Select(q => q.MaxLength).ToArray();
            }

            return new ChargeAccountValidationResult
            {
                Prompts = prompts,
                PromptsLength = promptsLength,
                ChargeTypeKeyOverride = chargeTypeOverride,
                IsChargeAccountPaymentWithCardOnFile = isChargeAccountPaymentWithCardOnFile
            };
        }

        private InitializePayPalCheckoutResponse InitializePayPalCheckoutIfNecessary(Guid accountId, bool isPrepaid, Guid orderId, CreateOrder request)
        {
            if (isPrepaid
                && request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
            {
                var paypalWebPaymentResponse = _payPalServiceFactory.GetInstance().InitializeWebPayment(accountId, orderId, Request.AbsoluteUri, request.Estimate.Price, request.ClientLanguageCode);

                if (paypalWebPaymentResponse.IsSuccessful)
                {
                    return paypalWebPaymentResponse;
                }

                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), paypalWebPaymentResponse.Message);
            }

            return null;
        }

        private void ValidateProvider(CreateOrder request, ReferenceData referenceData, bool isInExternalMarket)
        {
            // Provider is optional for home market
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (!isInExternalMarket
                && request.Settings.ProviderId.HasValue
                && referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_InvalidProvider.ToString(), 
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_InvalidProvider, request.ClientLanguageCode));
            }
        }


        private int CreateIbsAccountIfNeeded(AccountDetail account, string companyKey = null)
        {
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, companyKey);
            if (ibsAccountId.HasValue)
            {
                return ibsAccountId.Value;
            }

            // Account doesn't exist, create it
            ibsAccountId = _ibsServiceProvider.Account(companyKey).CreateAccount(account.Id,
                account.Email,
                string.Empty,
                account.Name,
                account.Settings.Phone);

            _commandBus.Send(new LinkAccountToIbs
            {
                AccountId = account.Id,
                IbsAccountId = ibsAccountId.Value,
                CompanyKey = companyKey
            });

            return ibsAccountId.Value;
        }

        private void ValidatePayment(CreateOrder request, Guid orderId, AccountDetail account, bool isFutureBooking, double? appEstimate, bool isPrepaid)
        {
            var tipPercent = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;

            // If there's an estimate, add tip to that estimate
            if (appEstimate.HasValue)
            {
                appEstimate = appEstimate.Value + FareHelper.CalculateTipAmount(appEstimate.Value, tipPercent);
            }

            var appEstimateWithTip = appEstimate.HasValue ? Convert.ToDecimal(appEstimate.Value) : (decimal?)null;

            if (isPrepaid)
            {
                // Verify that prepaid is enabled on the server
                if (!_serverSettings.GetPaymentSettings().IsPrepaidEnabled)
                {
                    throw new HttpError(HttpStatusCode.BadRequest,
                        ErrorCode.CreateOrder_RuleDisable.ToString(),
                         _resources.Get("CannotCreateOrder_PrepaidButPrepaidNotEnabled", request.ClientLanguageCode));
                }

                // PayPal is handled elsewhere since it has a different behavior

                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    if (!appEstimateWithTip.HasValue)
                    {
                        throw new HttpError(HttpStatusCode.BadRequest,
                            ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrder_PrepaidNoEstimate", request.ClientLanguageCode));
                    }
                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv);
                    CapturePaymentForPrepaidOrder(orderId, account, Convert.ToDecimal(appEstimateWithTip), tipPercent, request.Cvv);
                }
            }
            else
            {
                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv);
                    PreAuthorizePaymentMethod(orderId, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip, false, request.Cvv);
                }

                // Payment mode is PayPal
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.PayPal.Id)
                {
                    ValidatePayPal(orderId, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip);
                }
            }
        }

        private void ValidateCreditCard(AccountDetail account, string clientLanguageCode, string cvv)
        {
            // check if the account has a credit card
            if (!account.DefaultCreditCard.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileButNoCreditCard.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_CardOnFileButNoCreditCard, clientLanguageCode));
            }

            var creditCard = _creditCardDao.FindByAccountId(account.Id).First();
            if (creditCard.IsExpired())
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_CreditCardExpired", clientLanguageCode));
            }
            if (creditCard.IsDeactivated)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileDeactivated.ToString(),
                    _resources.Get("CannotCreateOrder_CreditCardDeactivated", clientLanguageCode));
            }

            if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking
                && !cvv.HasValue())
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_CreditCardCvvRequired", clientLanguageCode));
            }
        }

        private void ValidatePayPal(Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimate)
        {
            if (!_serverSettings.GetPaymentSettings().PayPalClientSettings.IsEnabled
                    || !account.IsPayPalAccountLinked)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_PayPalButNoPayPal", clientLanguageCode));
            }

            PreAuthorizePaymentMethod(orderId, account, clientLanguageCode, isFutureBooking, appEstimate, true);
        }

        private void PreAuthorizePaymentMethod(Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimate, bool isPayPal, string cvv = null)
        {
            if (!_serverSettings.GetPaymentSettings().IsPreAuthEnabled || isFutureBooking)
            {
                // preauth will be done later, save the info temporarily
                if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking)
                {
                    _commandBus.Send(new SaveTemporaryOrderPaymentInfo { OrderId = orderId, Cvv = cvv });
                }
                
                return;
            }
            
            // there's a minimum amount of $50 (warning indicating that on the admin ui)
            // if app returned an estimate, use it, otherwise use the setting (or 0), then use max between the value and 50
            var preAuthAmount = Math.Max(appEstimate ?? (_serverSettings.GetPaymentSettings().PreAuthAmount ?? 0), 50);

            var preAuthResponse = _paymentService.PreAuthorize(orderId, account, preAuthAmount, cvv: cvv);

            var errorMessage = isPayPal
                ? _resources.Get("CannotCreateOrder_PayPalWasDeclined", clientLanguageCode)
                : _resources.Get("CannotCreateOrder_CreditCardWasDeclined", clientLanguageCode);

            if (!preAuthResponse.IsSuccessful)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), errorMessage);
            }
        }
        
        private void ValidateAppVersion(string clientLanguage)
        {
            var appVersion = base.Request.Headers.Get("ClientVersion");
            var minimumAppVersion = _serverSettings.ServerData.MinimumRequiredAppVersion;

            if (appVersion.IsNullOrEmpty() || minimumAppVersion.IsNullOrEmpty())
            {
                return;
            }

            var minimumMajorMinorBuild = minimumAppVersion.Split(new[]{ "." }, StringSplitOptions.RemoveEmptyEntries);
            var appMajorMinorBuild = appVersion.Split('.');

            for (var i = 0; i < appMajorMinorBuild.Length; i++)
            {
                var appVersionItem = int.Parse(appMajorMinorBuild[i]);
                var minimumVersionItem = int.Parse(minimumMajorMinorBuild.Length <= i ? "0" : minimumMajorMinorBuild[i]);

                if (appVersionItem < minimumVersionItem)
                {
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                                        _resources.Get("CannotCreateOrderInvalidVersion", clientLanguage));
                }
            }
        }

        private void ValidateChargeAccountAnswers(string accountNumber, string customerNumber, AccountChargeQuestion[] userQuestionsDetails, string clientLanguageCode)
        {
            var accountChargeDetail = _accountChargeDao.FindByAccountNumber(accountNumber);
            if (accountChargeDetail == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.AccountCharge_InvalidAccountNumber.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.AccountCharge_InvalidAccountNumber, clientLanguageCode));
            }

            var answers = userQuestionsDetails.Select(x => x.Answer);

            var validation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(answers, accountNumber, customerNumber);
            if (!validation.Valid)
            {
                if (validation.ValidResponse != null)
                {
                    int firstError = validation.ValidResponse.IndexOf(false);
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.AccountCharge_InvalidAnswer.ToString(),
                                            accountChargeDetail.Questions[firstError].ErrorMessage);
                }

                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.AccountCharge_InvalidAccountNumber.ToString(), validation.Message);
            }
        }

        private DateTime GetCurrentOffsetedTime(string companyKey)
        {
            //TODO : need to check ibs setup for shortesst time

            var ibsServerTimeDifference = _ibsServiceProvider.GetSettingContainer(companyKey).TimeDifference;
            var offsetedTime = DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }

        private string BuildNote(string chargeType, string note, string buildingName, int largeBags)
        {
            // Building Name is not handled by IBS
            // Put Building Name in note, if specified

            // Get NoteTemplate from app settings, if it exists
            var noteTemplate = _serverSettings.ServerData.IBS.NoteTemplate;

            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                // Quickfix: If the address comes from our Google Places service
                // the building name will be formatted like this: "Building Name (Place Type)"
                // We need to remove the text in parenthesis

                const string pattern = @"
\(         # Look for an opening parenthesis
[^\)]+     # Take all characters that are not a closing parenthesis
\)$        # Look for a closing parenthesis at the end of the string";

                buildingName = new Regex(pattern, RegexOptions.IgnorePatternWhitespace)
                    .Replace(buildingName, string.Empty).Trim();
                buildingName = "Building name: " + buildingName;
            }

            var largeBagsString = string.Empty;
            if (largeBags > 0)
            {
                largeBagsString = "Large bags: " + largeBags;
            }

            if (!string.IsNullOrWhiteSpace(noteTemplate))
            {
                if (!_serverSettings.ServerData.IBS.HideChargeTypeInUserNote)
                {
                    noteTemplate = string.Format("{0}{1}{2}",
                        chargeType,
                        Environment.NewLine,
                        noteTemplate);
                }
                
                var transformedTemplate = noteTemplate
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("{{userNote}}", note ?? string.Empty)
                    .Replace("{{buildingName}}", buildingName ?? string.Empty)
                    .Replace("{{largeBags}}", largeBagsString)
                    .Trim();

                return transformedTemplate;
            }

            // In versions prior to 1.4, there was no note template
            // So if the IBS.NoteTemplate setting does not exist, use the old way 
            var formattedNote = string.Format("{0}{0}{1}{2}{3}", 
                    Environment.NewLine, 
                    chargeType,
                    Environment.NewLine,
                    note);

            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                formattedNote += (Environment.NewLine + buildingName).Trim();
            }
            // "Large bags" appeared in 1.4, no need to concat it here
            return formattedNote;
        }

        private Fare GetFare(CreateOrder.RideEstimate estimate)
        {
            if (estimate == null || !estimate.Price.HasValue)
            {
                return default(Fare);
            }

            return FareHelper.GetFareFromAmountInclTax(estimate.Price.Value,
                _serverSettings.ServerData.VATIsEnabled
                    ? _serverSettings.ServerData.VATPercentage
                    : 0);
        }

        private Guid? GetPendingOrder()
        {
            var activeOrders = _orderDao.GetOrdersInProgressByAccountId(new Guid(this.GetSession().UserAuthId));

            var latestActiveOrder = activeOrders.FirstOrDefault(o => o.IBSStatusId != VehicleStatuses.Common.Scheduled);
            if (latestActiveOrder != null)
            {
                return latestActiveOrder.OrderId;
            }

            return null;
        }

        private BestAvailableCompany FindBestAvailableCompany(string market, double? latitude, double? longitude)
        {
            if (!market.HasValue() || !latitude.HasValue || !longitude.HasValue)
            {
                // Do nothing if in home market or if we don't have position
                return new BestAvailableCompany();
            }

            int? bestFleetId = null;
            const int searchExpendLimit = 10;
            var searchRadius = 2000; // In meters

            for (var i = 1; i < searchExpendLimit; i++)
            {
                var marketVehicles =
                    _honeyBadgerServiceClient.GetAvailableVehicles(market, latitude.Value, longitude.Value, searchRadius, null, true)
                                             .ToArray();

                if (marketVehicles.Any())
                {
                    // Group vehicles by fleet
                    var vehiclesGroupedByFleet = marketVehicles.GroupBy(v => v.FleetId).Select(g => g.ToArray()).ToArray();

                    // Take fleet with most number of available vehicles
                    bestFleetId = vehiclesGroupedByFleet.Aggregate(
                        (fleet1, fleet2) => fleet1.Length > fleet2.Length ? fleet1 : fleet2).First().FleetId;
                    break;
                }

                // Nothing found, extend search radius (total radius after 10 iterations: 3375m)
                searchRadius += (i * 25);
            }

            if (bestFleetId.HasValue)
            {
                var companyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;
                var marketFleets = _taxiHailNetworkServiceClient.GetMarketFleets(companyKey, market).ToArray();

                // Fallback: If for some reason, we cannot find a match for the best fleet id in the fleets
                // that were setup for the market, we take the first one
                var bestFleet = marketFleets.FirstOrDefault(f => f.FleetId == bestFleetId.Value)
                    ?? marketFleets.FirstOrDefault();

                return new BestAvailableCompany
                {
                    CompanyKey = bestFleet != null ? bestFleet.CompanyKey : null,
                    CompanyName = bestFleet != null ? bestFleet.CompanyName : null
                };
            }

            // Nothing found
            return new BestAvailableCompany();
        }

        private BestAvailableCompany FindSpecificCompany(string market, string orderCompanyKey = null, int? orderFleetId = null)
        {
            if (!orderCompanyKey.HasValue() && !orderFleetId.HasValue)
            {
                throw new ArgumentNullException("You must at least provide a value for orderCompanyKey or orderFleetId");
            }

            var companyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;
            var marketFleets = _taxiHailNetworkServiceClient.GetMarketFleets(companyKey, market).ToArray();

            if (orderCompanyKey.HasValue())
            {
                var match = marketFleets.FirstOrDefault(f => f.CompanyKey == orderCompanyKey);
                if (match != null)
                {
                    return new BestAvailableCompany
                    {
                        CompanyKey = match.CompanyKey,
                        CompanyName = match.CompanyName
                    };
                }
            }

            if (orderFleetId.HasValue)
            {
                var match = marketFleets.FirstOrDefault(f => f.FleetId == orderFleetId.Value);
                if (match != null)
                {
                    return new BestAvailableCompany
                    {
                        CompanyKey = match.CompanyKey,
                        CompanyName = match.CompanyName
                    };
                }
            }

            // Nothing found
            return new BestAvailableCompany();
        }

        private void UpdateVehicleTypeFromMarketData(BookingSettings bookingSettings, string marketCompanyId)
        {
            if (!bookingSettings.VehicleTypeId.HasValue)
            {
                // Nothing to do
                return;
            }

            try
            {
                // Get the vehicle type defined for the market of the company
                var matchingMarketVehicle = _taxiHailNetworkServiceClient.GetAssociatedMarketVehicleType(marketCompanyId, bookingSettings.VehicleTypeId.Value);
                if (matchingMarketVehicle != null)
                {
                    // Update the vehicle type info using the vehicle id from the IBS of that company
                    bookingSettings.VehicleType = matchingMarketVehicle.Name;
                    bookingSettings.VehicleTypeId = matchingMarketVehicle.ReferenceDataVehicleId;
                }
                else
                {
                    // No match found
                    bookingSettings.VehicleType = null;
                    bookingSettings.VehicleTypeId = null;

                    Log.Info(string.Format("No match found for GetAssociatedMarketVehicleType for company {0}. Maybe no vehicles were linked via the admin panel?", marketCompanyId));
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("An error occurred when trying to get GetAssociatedMarketVehicleType for company {0}", marketCompanyId));
                Log.Error(ex);
            }
        }

        private Guid? ValidatePromotion(string promoCode, int? chargeTypeId, Guid accountId, Guid orderId, DateTime pickupDate, bool isFutureBooking, string clientLanguageCode)
        {
            if (!promoCode.HasValue())
            {
                // No promo code entered
                return null;
            }

            var usingPaymentInApp = chargeTypeId == ChargeTypes.CardOnFile.Id || chargeTypeId == ChargeTypes.PayPal.Id;
            if (!usingPaymentInApp)
            {
                var payPalIsEnabled = _serverSettings.GetPaymentSettings().PayPalClientSettings.IsEnabled;
                var cardOnFileIsEnabled = _serverSettings.GetPaymentSettings().IsPayInTaxiEnabled;

                var promotionErrorResourceKey = "CannotCreateOrder_PromotionMustUseCardOnFile";
                if (payPalIsEnabled && cardOnFileIsEnabled)
                {
                    promotionErrorResourceKey = "CannotCreateOrder_PromotionMustUseCardOnFileOrPayPal";
                }
                else if (payPalIsEnabled)
                {
                    promotionErrorResourceKey = "CannotCreateOrder_PromotionMustUsePayPal";
                }
                
                // Should never happen since we will check client-side if there's a promocode and not paying with CoF/PayPal
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get(promotionErrorResourceKey, clientLanguageCode));
            }

            var promo = _promotionDao.FindByPromoCode(promoCode);
            if (promo == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get("CannotCreateOrder_PromotionDoesNotExist", clientLanguageCode));
            }

            var promoDomainObject = _promoRepository.Get(promo.Id);
            string errorMessage;
            if (!promoDomainObject.CanApply(accountId, pickupDate, isFutureBooking, out errorMessage))
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get(errorMessage, clientLanguageCode));
            }

            return promo.Id;
        }

        private string GetCreateOrderServiceErrorMessage(ErrorCode errorCode, string language)
        {
            var callMessage = string.Format(_resources.Get("ServiceError" + errorCode, language),
                _serverSettings.ServerData.TaxiHail.ApplicationName,
                _serverSettings.ServerData.DefaultPhoneNumberDisplay);

            var noCallMessage = _resources.Get("ServiceError" + errorCode + "_NoCall", language);

            return _serverSettings.ServerData.HideCallDispatchButton ? noCallMessage : callMessage;
        }

        private void CapturePaymentForPrepaidOrder(Guid orderId, AccountDetail account, decimal appEstimateWithTip, int tipPercentage, string cvv)
        {
            // Note: No promotion on web
            var tipAmount = FareHelper.GetTipAmountFromTotalIncludingTip(appEstimateWithTip, tipPercentage);
            var totalAmount = appEstimateWithTip;
            var meterAmount = totalAmount - tipAmount;

            var preAuthResponse = _paymentService.PreAuthorize(orderId, account, totalAmount, isForPrepaid: true, cvv: cvv);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    orderId,
                    account,
                    totalAmount,
                    totalAmount,
                    meterAmount,
                    tipAmount,
                    preAuthResponse.TransactionId,
                    preAuthResponse.ReAuthOrderId,
                    isForPrepaid: true);

                if (commitResponse.IsSuccessful)
                {
                    var paymentDetail = _orderPaymentDao.FindByOrderId(orderId);

                    var fareObject = FareHelper.GetFareFromAmountInclTax(meterAmount,
                        _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(orderId),
                        Amount = totalAmount,
                        MeterAmount = fareObject.AmountExclTax,
                        TipAmount = tipAmount,
                        TaxAmount = fareObject.TaxAmount,
                        AuthorizationCode = commitResponse.AuthorizationCode,
                        TransactionId = commitResponse.TransactionId,
                        IsForPrepaidOrder = true
                    });
                }
                else
                {
                    // Payment failed, void preauth
                    _paymentService.VoidPreAuthorization(orderId, true);

                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), commitResponse.Message);
                }
            }
            else
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), preAuthResponse.Message);
            }
        }

        private class ChargeAccountValidationResult
        {
            public string[] Prompts { get; set; }

            public int?[] PromptsLength { get; set; }

            public string ChargeTypeKeyOverride { get; set; }

            public bool IsChargeAccountPaymentWithCardOnFile { get; set; }
        }
    }
}