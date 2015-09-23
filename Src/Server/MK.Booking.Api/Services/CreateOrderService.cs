#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
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
using CMTServices;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using log4net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using CreateOrder = apcurium.MK.Booking.Api.Contract.Requests.CreateOrder;
using apcurium.MK.Common.Helpers;

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
        private readonly IFeesDao _feesDao;
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
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
            IOrderPaymentDao orderPaymentDao,
            IFeesDao feesDao)
        {
            _accountChargeDao = accountChargeDao;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _ruleCalculator = ruleCalculator;
            _updateOrderStatusJob = updateOrderStatusJob;
            _orderDao = orderDao;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _honeyBadgerServiceClient = honeyBadgerServiceClient;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _paymentService = paymentService;
            _payPalServiceFactory = payPalServiceFactory;
            _orderPaymentDao = orderPaymentDao;
            _feesDao = feesDao;
            _resources = new Resources.Resources(_serverSettings);
        }

        public object Post(CreateOrder request)
        {
            return CreaterOrder(request, false);
        }

        public object Post(HailRequest request)
        {
            Log.Info(string.Format("Starting Hail. Request : {0}", request.ToJson()));

            var createOrderRequest = Mapper.Map<CreateOrder>(request);

            return CreaterOrder(createOrderRequest, true);
        }

        public object Post(ConfirmHailRequest request)
        {
            var orderDetail = _orderDao.FindById(request.OrderKey.Guid);
            if (orderDetail == null)
            {
                throw new HttpError(string.Format("Order {0} doesn't exist", request.OrderKey.Guid));
            }

            Log.Info(string.Format("Trying to confirm Hail. Request : {0}", request.ToJson()));

            var ibsOrderKey = Mapper.Map<IbsOrderKey>(request.OrderKey);
            var ibsVehicleCandidate = Mapper.Map<IbsVehicleCandidate>(request.VehicleCandidate);

            var confirmHailResult = _ibsServiceProvider.Booking(orderDetail.CompanyKey).ConfirmHail(ibsOrderKey, ibsVehicleCandidate);
            if (confirmHailResult < 0)
            {
                var errorMessage = string.Format("Error while trying to confirm the hail. IBS response code : {0}", confirmHailResult);
                Log.Error(errorMessage);

                return new HttpResult(HttpStatusCode.InternalServerError, errorMessage);
            }

            Log.Info("Hail request confirmed");

            return new HttpResult(HttpStatusCode.OK);
        }

        private object CreaterOrder(CreateOrder request, bool isHailRequest)
        {
			var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

			var createReportOrder = CreateReportOrder(request, account);

			Exception createOrderException;

            Log.Info("Create order request : " + request.ToJson());

            var countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Settings.Country));

            if (countryCode.IsNumberPossible(request.Settings.Phone))
            {
                request.Settings.Phone = PhoneHelper.GetDigitsFromPhoneNumber(request.Settings.Phone);
            }
            else
            {
                createOrderException = new HttpError(string.Format(_resources.Get("PhoneNumberFormat"), countryCode.GetPhoneExample()));
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            // TODO: Find a better way to do this...
            var isFromWebApp = request.FromWebApp;

            if (!isFromWebApp)
            {
                ValidateAppVersion(request.ClientLanguageCode, createReportOrder);
            }

            // Find market
            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            market = market.HasValue() ? market : null;

			createReportOrder.Market = market;

            BestAvailableCompany bestAvailableCompany;

            if (request.OrderCompanyKey.HasValue() || request.OrderFleetId.HasValue)
            {
                // For API user, it's possible to manually specify which company to dispatch to by using a fleet id
                bestAvailableCompany = FindSpecificCompany(market, createReportOrder, request.OrderCompanyKey, request.OrderFleetId);
            }
            else
            {
                bestAvailableCompany = FindBestAvailableCompany(market, request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }

			createReportOrder.CompanyKey = bestAvailableCompany.CompanyKey;
			createReportOrder.CompanyName = bestAvailableCompany.CompanyName;

            if (market.HasValue() && !bestAvailableCompany.CompanyKey.HasValue())
            {
                // No companies available that are desserving this region for the company
				createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrder_NoCompanies", request.ClientLanguageCode));
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}

            UpdateVehicleTypeFromMarketData(request.Settings, bestAvailableCompany.CompanyKey);

            if (market.HasValue())
            {
                var isConfiguredForCmtPayment = FetchCompanyPaymentSettings(bestAvailableCompany.CompanyKey);
                if (!isConfiguredForCmtPayment)
                {
                    // Only companies configured for CMT payment can support CoF orders outside of home market
                    request.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                }
            }

            var isPrepaid = isFromWebApp
                && (request.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                    || request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id);

			createReportOrder.IsPrepaid = isPrepaid;

            account.IBSAccountId = CreateIbsAccountIfNeeded(account, bestAvailableCompany.CompanyKey);
            
            var isFutureBooking = request.PickupDate.HasValue;
            var pickupDate = request.PickupDate ?? GetCurrentOffsetedTime(bestAvailableCompany.CompanyKey);

			createReportOrder.PickupDate = pickupDate;

            // User can still create future order, but we allow only one active Book now order.
            if (!isFutureBooking)
            {
                var pendingOrderId = GetPendingOrder();

                // We don't allow order creation if there's already an order scheduled
                if (!_serverSettings.ServerData.AllowSimultaneousAppOrders
                    && pendingOrderId != null
                    && !isFromWebApp)
                {
                    createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_PendingOrder.ToString(), pendingOrderId.ToString());
					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
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
                createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
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
                    createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), validationResult.Message);
					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
				}
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                createOrderException = new HttpError(ErrorCode.CreateOrder_SettingsRequired.ToString());
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
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
                createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_NoFareEstimateAvailable.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_NoFareEstimateAvailable, request.ClientLanguageCode));
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            // IBS provider validation
            ValidateProvider(request, referenceData, market.HasValue(), createReportOrder);

            // Map the command to obtain a OrderId (web doesn't prepopulate it in the request)
            var orderCommand = Mapper.Map<Commands.CreateOrder>(request);

            var marketFees = _feesDao.GetMarketFees(market);
            orderCommand.BookingFees = marketFees != null ? marketFees.Booking : 0;
			createReportOrder.BookingFees = orderCommand.BookingFees;

            // Promo code validation
            var applyPromoCommand = ValidateAndApplyPromotion(bestAvailableCompany.CompanyKey, request.PromoCode, request.Settings.ChargeTypeId, account.Id, orderCommand.OrderId, pickupDate, isFutureBooking, request.ClientLanguageCode, createReportOrder);

            // Charge account validation
            var accountValidationResult = ValidateChargeAccountIfNecessary(bestAvailableCompany.CompanyKey, request, orderCommand.OrderId, account, isFutureBooking, isFromWebApp, orderCommand.BookingFees, createReportOrder);
			createReportOrder.IsChargeAccountPaymentWithCardOnFile = accountValidationResult.IsChargeAccountPaymentWithCardOnFile;

            // if ChargeAccount uses payment with card on file, payment validation was already done
            if (!accountValidationResult.IsChargeAccountPaymentWithCardOnFile)
            {
                // Payment method validation
                ValidatePayment(bestAvailableCompany.CompanyKey, request, orderCommand.OrderId, account, isFutureBooking, request.Estimate.Price, orderCommand.BookingFees, isPrepaid, createReportOrder);
            }
            
            // Initialize PayPal if user is using PayPal web
            var  paypalWebPaymentResponse = InitializePayPalCheckoutIfNecessary(account.Id, isPrepaid, orderCommand.OrderId, request, orderCommand.BookingFees, bestAvailableCompany.CompanyKey, createReportOrder);

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
                        Request = request,
                        ReferenceData = referenceData,
                        ChargeTypeIbs = chargeTypeIbs,
                        ChargeTypeEmail = chargeTypeEmail,
                        VehicleType = vehicleType,
                        Prompts = accountValidationResult.Prompts,
                        PromptsLength = accountValidationResult.PromptsLength,
                        BestAvailableCompany = bestAvailableCompany,
                        ApplyPromoCommand = applyPromoCommand
                    }.ToJson()
                });

                return paypalWebPaymentResponse;
            }

            // Create order on IBS
            if (isHailRequest)
            {
                var result = CreateIBSHailOrder(orderCommand.OrderId, account, request, referenceData, chargeTypeIbs,
                                accountValidationResult.Prompts, accountValidationResult.PromptsLength,
                                bestAvailableCompany, market, isPrepaid).Result;

                return result.HailResult;
            }
            else
            {
                Task.Run(() =>
                    CreateOrderOnIBSAndSendCommands(orderCommand.OrderId, account,
                        request, referenceData, chargeTypeIbs, chargeTypeEmail, vehicleType,
                        accountValidationResult.Prompts, accountValidationResult.PromptsLength,
                        bestAvailableCompany, applyPromoCommand, market, isPrepaid));
            }

            if (request.QuestionsAndAnswers.HasValue())
            {
                // Save question answers so we can display them the next time the user books
                var accountLastAnswers = request.QuestionsAndAnswers
                  .Where(q => q.SaveAnswer)
                  .Select(q =>
                      new AccountChargeQuestionAnswer
                      {
                          AccountId = account.Id,
                          AccountChargeQuestionId = q.Id,
                          AccountChargeId = q.AccountId,
                          LastAnswer = q.Answer
                      });

                if (accountLastAnswers != null)
                {
                    _commandBus.Send(new AddUpdateAccountQuestionAnswer { AccountId = account.Id, Answers = accountLastAnswers.ToArray() });
                }  
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
            var orderInfo = JsonSerializer.DeserializeFromString<TemporaryOrderCreationInfo>(temporaryInfo.SerializedOrderCreationInfo);

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
                var response = _payPalServiceFactory.GetInstance(orderInfo.BestAvailableCompany.CompanyKey).ExecuteWebPayment(request.PayerId, request.PaymentId);

                if (response.IsSuccessful)
                {
                    var fareObject = FareHelper.GetFareFromAmountInclTax(Convert.ToDouble(orderInfo.Request.Estimate.Price),
                        _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    var tipPercentage = orderInfo.Account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;
                    var tipAmount = FareHelper.CalculateTipAmount(fareObject.AmountInclTax, tipPercentage);

                    _commandBus.Send(new MarkPrepaidOrderAsSuccessful
                    {
                        OrderId = request.OrderId,
                        TotalAmount = fareObject.AmountInclTax + tipAmount,
                        MeterAmount = fareObject.AmountExclTax,
                        TaxAmount = fareObject.TaxAmount,
                        TipAmount = tipAmount,
                        TransactionId = response.TransactionId,
                        Provider = PaymentProvider.PayPal,
                        Type = PaymentType.PayPal
                    });

                    // Create order on IBS
                    Task.Run(() => CreateOrderOnIBSAndSendCommands(orderInfo.OrderId, orderInfo.Account, orderInfo.Request, orderInfo.ReferenceData,
                        orderInfo.ChargeTypeIbs, orderInfo.ChargeTypeEmail, orderInfo.VehicleType, orderInfo.Prompts, orderInfo.PromptsLength,
                        orderInfo.BestAvailableCompany, orderInfo.ApplyPromoCommand, isPrepaid: true));
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

            var isConfiguredForCmtPayment = FetchCompanyPaymentSettings(request.NextDispatchCompanyKey);
            var chargeTypeId = order.Settings.ChargeTypeId;
            var chargeTypeDisplay = order.Settings.ChargeType;
            if (!isConfiguredForCmtPayment)
            {
                // Only companies configured for CMT payment can support CoF orders outside of home market
                chargeTypeId = ChargeTypes.PaymentInCar.Id;
                chargeTypeDisplay = ChargeTypes.PaymentInCar.Display;
            }

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

                    ChargeType = chargeTypeDisplay,
                    ChargeTypeId = chargeTypeId,

                    // Reset vehicle type
                    VehicleType = null,
                    VehicleTypeId = null
                },
                Note = order.UserNote,
                Estimate = new CreateOrder.RideEstimate { Price = order.EstimatedFare }
            };

            var newReferenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = request.NextDispatchCompanyKey });

            // This must be localized with the priceformat to be localized in the language of the company
            // because it is sent to the driver
            var chargeTypeIbs = _resources.Get(chargeTypeDisplay, _serverSettings.ServerData.PriceFormat);

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

            ValidateProvider(newOrderRequest, newReferenceData, market.HasValue(), null);

            var orderResult = CreateIbsOrder(ibsAccountId, newOrderRequest, newReferenceData, chargeTypeIbs, null, null, market, false, request.NextDispatchCompanyKey);

            var newIbsOrderId = orderResult.IsHailRequest
                ? orderResult.HailResult.OrderKey.OrderId
                : orderResult.CreateOrderResult;
            
            if (!newIbsOrderId.HasValue || newIbsOrderId <= 0)
            {
                var code = !newIbsOrderId.HasValue || (newIbsOrderId.Value >= -1) ? string.Empty : "_" + Math.Abs(newIbsOrderId.Value);
                Log.Error(string.Format("{0}. IBS error code: {1}", networkErrorMessage, code));
                throw new HttpError(HttpStatusCode.InternalServerError, networkErrorMessage);
            }

            // Cancel order on current company IBS
            CancelIbsOrder(order, account.Id);

            _commandBus.Send(new SwitchOrderToNextDispatchCompany
            {
                OrderId = request.OrderId,
                IBSOrderId = newIbsOrderId.Value,
                CompanyKey = request.NextDispatchCompanyKey,
                CompanyName = request.NextDispatchCompanyName,
                Market = order.Market,
                HasChangedBackToPaymentInCar = newOrderRequest.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id
            });

            return new OrderStatusDetail
            {
                OrderId = request.OrderId,
                Status = OrderStatus.Created,
                CompanyKey = request.NextDispatchCompanyKey,
                CompanyName = request.NextDispatchCompanyName,
                NextDispatchCompanyKey = null,
                NextDispatchCompanyName = null,
                IBSOrderId = newIbsOrderId,
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

		private ChargeAccountValidationResult ValidateChargeAccountIfNecessary(string companyKey, CreateOrder request, Guid orderId, AccountDetail account, bool isFutureBooking, bool isFromWebApp, decimal bookingFees, CreateReportOrder createReportOrder)
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
                    if (isFromWebApp)
                    {
                        // Charge account cannot support prepaid orders
                        Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrderChargeAccountNotSupportedOnWeb", request.ClientLanguageCode));

						createReportOrder.Error = createOrderException.ToString();
						_commandBus.Send(createReportOrder);
						throw createOrderException;
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

                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.Settings.CustomerNumber, request.QuestionsAndAnswers, request.ClientLanguageCode, createReportOrder);

                if (isChargeAccountPaymentWithCardOnFile)
                {
                    ValidatePayment(companyKey, request, orderId, account, isFutureBooking, request.Estimate.Price, bookingFees, false, createReportOrder);
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

		private InitializePayPalCheckoutResponse InitializePayPalCheckoutIfNecessary(Guid accountId, bool isPrepaid, Guid orderId, CreateOrder request, decimal bookingFees, string companyKey, CreateReportOrder createReportOrder)
        {
            if (isPrepaid
                && request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
            {
                var paypalWebPaymentResponse = _payPalServiceFactory.GetInstance(companyKey).InitializeWebPayment(accountId, orderId, Request.AbsoluteUri, request.Estimate.Price, bookingFees, request.ClientLanguageCode);

                if (paypalWebPaymentResponse.IsSuccessful)
                {
                    return paypalWebPaymentResponse;
                }

                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), paypalWebPaymentResponse.Message);

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            return null;
        }

        private async Task<IBSOrderResult> CreateIBSHailOrder(Guid orderId, AccountDetail account, CreateOrder request,
            ReferenceData referenceData, string chargeTypeIbs, string[] prompts, int?[] promptsLength,
            BestAvailableCompany bestAvailableCompany, string market = null, bool isPrepaid = false)
        {
            var orderResult = CreateIbsOrder(account.IBSAccountId.Value, request, referenceData, chargeTypeIbs, prompts, promptsLength, market, true, bestAvailableCompany.CompanyKey);

            // Wait for order creation to complete before sending other commands
            await Task.Delay(750);

            ReactToIbsOrderCreation(orderId, orderResult.HailResult.OrderKey.OrderId, isPrepaid, request.ClientLanguageCode);

            UpdateStatusAsync(orderId);

            return orderResult;
        }

        private async void CreateOrderOnIBSAndSendCommands(Guid orderId, AccountDetail account, CreateOrder request, ReferenceData referenceData, 
            string chargeTypeIbs, string chargeTypeEmail, string vehicleType, string[] prompts, int?[] promptsLength, BestAvailableCompany bestAvailableCompany, 
            ApplyPromotion applyPromoCommand, string market = null, bool isPrepaid = false)
        {
            var orderResult = CreateIbsOrder(account.IBSAccountId.Value, request, referenceData, chargeTypeIbs, prompts, promptsLength, market, false, bestAvailableCompany.CompanyKey);

            // Wait for order creation to complete before sending other commands
            await Task.Delay(750);

            var ibsOrderId = orderResult.CreateOrderResult;

            var hasErrors = ReactToIbsOrderCreation(orderId, ibsOrderId, isPrepaid, request.ClientLanguageCode);
            if (!hasErrors)
            {
                var emailCommand = Mapper.Map<SendBookingConfirmationEmail>(request);
                emailCommand.IBSOrderId = ibsOrderId.Value;
                emailCommand.EmailAddress = account.Email;
                emailCommand.Settings.ChargeType = chargeTypeEmail;
                emailCommand.Settings.VehicleType = vehicleType;
                _commandBus.Send(emailCommand);

                if (applyPromoCommand != null)
                {
                    _commandBus.Send(applyPromoCommand);
                }
            }
 
            UpdateStatusAsync(orderId);
        }

        private bool ReactToIbsOrderCreation(Guid orderId, int? ibsOrderId, bool isPrepaid, string clientLanguageCode)
        {
            if (!ibsOrderId.HasValue || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? String.Empty : "_" + Math.Abs(ibsOrderId.Value);
                var errorCode = ErrorCode.CreateOrder_CannotCreateInIbs + code;

                var errorCommand = new CancelOrderBecauseOfError
                {
                    OrderId = orderId,
                    WasPrepaid = isPrepaid,
                    ErrorCode = errorCode,
                    ErrorDescription = _resources.Get(errorCode, clientLanguageCode)
                };

                _commandBus.Send(errorCommand);
                return false;
            }

            var ibsCommand = new AddIbsOrderInfoToOrder
            {
                OrderId = orderId,
                IBSOrderId = ibsOrderId.Value
            };
            _commandBus.Send(ibsCommand);

            return true;
        }

		private void ValidateProvider(CreateOrder request, ReferenceData referenceData, bool isInExternalMarket, CreateReportOrder createReportOrder)
        {
            // Provider is optional for home market
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (!isInExternalMarket
                && request.Settings.ProviderId.HasValue
                && referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {

                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_InvalidProvider.ToString(), 
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_InvalidProvider, request.ClientLanguageCode));

				if (createReportOrder != null)
				{
					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
				}
				throw createOrderException;
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

		private void ValidatePayment(string companyKey, CreateOrder request, Guid orderId, AccountDetail account, bool isFutureBooking, double? appEstimate, decimal bookingFees, bool isPrepaid, CreateReportOrder createReportOrder)
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
                if (!_serverSettings.GetPaymentSettings(companyKey).IsPrepaidEnabled)
                {
                    Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                        ErrorCode.CreateOrder_RuleDisable.ToString(),
                         _resources.Get("CannotCreateOrder_PrepaidButPrepaidNotEnabled", request.ClientLanguageCode));

					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
                }

                // PayPal is handled elsewhere since it has a different behavior

                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    if (!appEstimateWithTip.HasValue)
                    {
                        Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                            ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrder_PrepaidNoEstimate", request.ClientLanguageCode));

						createReportOrder.Error = createOrderException.ToString();
						_commandBus.Send(createReportOrder);
						throw createOrderException;
					}
                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv, createReportOrder);
                    CapturePaymentForPrepaidOrder(companyKey, orderId, account, Convert.ToDecimal(appEstimateWithTip), tipPercent, bookingFees, request.Cvv, createReportOrder);
                }
            }
            else
            {
                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv, createReportOrder);
                    PreAuthorizePaymentMethod(companyKey, orderId, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip, bookingFees, false, createReportOrder, request.Cvv);
                }

                // Payment mode is PayPal
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.PayPal.Id)
                {
                    ValidatePayPal(companyKey, orderId, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip, bookingFees, createReportOrder);
                }
            }
        }

		private void ValidateCreditCard(AccountDetail account, string clientLanguageCode, string cvv, CreateReportOrder createReportOrder)
        {
            // check if the account has a credit card
            if (!account.DefaultCreditCard.HasValue)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileButNoCreditCard.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_CardOnFileButNoCreditCard, clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            var creditCard = _creditCardDao.FindByAccountId(account.Id).First();
            if (creditCard.IsExpired())
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_CreditCardExpired", clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}
            if (creditCard.IsDeactivated)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileDeactivated.ToString(),
                    _resources.Get("CannotCreateOrder_CreditCardDeactivated", clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}

            if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking
                && !cvv.HasValue())
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_CreditCardCvvRequired", clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}
        }

        private void ValidatePayPal(string companyKey, Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimateWithTip, decimal bookingFees, CreateReportOrder createReportOrder)
        {
            if (!_serverSettings.GetPaymentSettings(companyKey).PayPalClientSettings.IsEnabled
                    || !account.IsPayPalAccountLinked)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_RuleDisable.ToString(),
                     _resources.Get("CannotCreateOrder_PayPalButNoPayPal", clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}

            PreAuthorizePaymentMethod(companyKey, orderId, account, clientLanguageCode, isFutureBooking, appEstimateWithTip, bookingFees, true, createReportOrder);
        }

        private void PreAuthorizePaymentMethod(string companyKey, Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimateWithTip, decimal bookingFees, bool isPayPal, CreateReportOrder createReportOrder, string cvv = null)
        {
            if (!_serverSettings.GetPaymentSettings(companyKey).IsPreAuthEnabled || isFutureBooking)
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
            if (appEstimateWithTip.HasValue)
            {
                appEstimateWithTip = appEstimateWithTip.Value + bookingFees;
            }

            var preAuthAmount = Math.Max(appEstimateWithTip ?? (_serverSettings.GetPaymentSettings(companyKey).PreAuthAmount ?? 0), 50);

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, account, preAuthAmount, cvv: cvv);

            var errorMessage = isPayPal
                ? _resources.Get("CannotCreateOrder_PayPalWasDeclined", clientLanguageCode)
                : _resources.Get("CannotCreateOrder_CreditCardWasDeclined", clientLanguageCode);

            if (!preAuthResponse.IsSuccessful)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), errorMessage);

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }
        }

		private void ValidateAppVersion(string clientLanguage, CreateReportOrder createReportOrder)
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
                    Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                                        _resources.Get("CannotCreateOrderInvalidVersion", clientLanguage));

					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
                }
            }
        }

		private void ValidateChargeAccountAnswers(string accountNumber, string customerNumber, AccountChargeQuestion[] userQuestionsDetails, string clientLanguageCode, CreateReportOrder createReportOrder)
        {
            var accountChargeDetail = _accountChargeDao.FindByAccountNumber(accountNumber);
            if (accountChargeDetail == null)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.AccountCharge_InvalidAccountNumber.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.AccountCharge_InvalidAccountNumber, clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            var answers = userQuestionsDetails.Select(x => x.Answer);

            var validation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(answers, accountNumber, customerNumber);
            if (!validation.Valid)
            {
                if (validation.ValidResponse != null)
                {
                    int firstError = validation.ValidResponse.IndexOf(false);
                    Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.AccountCharge_InvalidAnswer.ToString(),
                                            accountChargeDetail.Questions[firstError].ErrorMessage);

					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
				}

                Exception createOrderException1 = new HttpError(HttpStatusCode.BadRequest, ErrorCode.AccountCharge_InvalidAccountNumber.ToString(), validation.Message);
				
				createReportOrder.Error = createOrderException1.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException1;
			}
        }

        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(() =>
            {
                _updateOrderStatusJob.CheckStatus(orderId);
            });
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

        private IBSOrderResult CreateIbsOrder(int ibsAccountId, CreateOrder request, ReferenceData referenceData, string chargeType, string[] prompts, int?[] promptsLength, string market, bool isHailRequest, string companyKey = null)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                return new IBSOrderResult
                {
                    CreateOrderResult = new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000),
                    IsHailRequest = isHailRequest
                };
            }

            var defaultCompany = referenceData.CompaniesList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceData.CompaniesList.FirstOrDefault();

            var providerId = market.HasValue() && referenceData.CompaniesList.Any() && defaultCompany != null
                    ? defaultCompany.Id
                    : request.Settings.ProviderId;

            var ibsPickupAddress = Mapper.Map<IbsAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress)
                ? Mapper.Map<IbsAddress>(request.DropOffAddress)
                : null;

            var promoCode =
                request.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id     // promo only applied if payment with CoF/PayPal
                    || request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id
                        ? request.PromoCode 
                        : null; 
            var note = BuildNote(chargeType, request.Note, request.PickupAddress.BuildingName, request.Settings.LargeBags, promoCode);
            var fare = GetFare(request.Estimate);

            Debug.Assert(request.PickupDate != null, "request.PickupDate != null");

            // This needs to be null if not set or the payment in car payment type id of ibs
            // It might not always be the correct value since when we're dispatching to another company,
            // we're passing the same ibs charge type id.  Since it's been this way for long, we assume it's working
            int? ibsChargeTypeId;
            if (request.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                || request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId;
            }
            else if (request.Settings.ChargeTypeId == ChargeTypes.Account.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeChargeAccountId;
            }
            else
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId;
            }

            var customerNumber = GetCustomerNumber(request.Settings.AccountNumber, request.Settings.CustomerNumber);

            int? createOrderResult = null;
            IbsHailResponse ibsHailResult = null;

            if (isHailRequest)
            {
                ibsHailResult = _ibsServiceProvider.Booking(companyKey).Hail(
                    providerId,
                    ibsAccountId,
                    request.Settings.Name,
                    CountryCode.GetCountryCodeByIndex(
                        CountryCode.GetCountryCodeIndexByCountryISOCode(request.Settings.Country)).CountryDialCode +
                    request.Settings.Phone,
                    request.Settings.Passengers,
                    request.Settings.VehicleTypeId,
                    ibsChargeTypeId,
                    note,
                    request.PickupDate.Value,
                    ibsPickupAddress,
                    ibsDropOffAddress,
                    request.Settings.AccountNumber,
                    customerNumber,
                    prompts,
                    promptsLength,
                    fare);
            }
            else
            {
                createOrderResult = _ibsServiceProvider.Booking(companyKey).CreateOrder(
                    providerId,
                    ibsAccountId,
                    request.Settings.Name,
                    CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Settings.Country)).CountryDialCode + request.Settings.Phone,
                    request.Settings.Passengers,
                    request.Settings.VehicleTypeId,
                    ibsChargeTypeId,                    
                    note,
                    request.PickupDate.Value,
                    ibsPickupAddress,
                    ibsDropOffAddress,
                    request.Settings.AccountNumber,
                    customerNumber,
                    prompts,
                    promptsLength,
                    fare);
            }

            var hailResult = Mapper.Map<OrderHailResult>(ibsHailResult);

            return new IBSOrderResult
            {
                CreateOrderResult = createOrderResult,
                HailResult = hailResult,
                IsHailRequest = isHailRequest
            };
        }

        private int? GetCustomerNumber(string accountNumber, string customerNumber)
        {
            if (!accountNumber.HasValue() || !customerNumber.HasValue())
            {
                return null;
            }

            int result;
            if (int.TryParse(customerNumber, out result))
            {
                return result;
            }

            return null;
        }

        private void CancelIbsOrder(OrderDetail order, Guid accountId)
        {
            // Cancel order on current company IBS
            if (order.IBSOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(accountId, order.CompanyKey);
                if (currentIbsAccountId.HasValue)
                {
                    // We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride...
                    // After 5 time, we are giving up. But we assume the order is completed.
                    Task.Factory.StartNew(() =>
                    {
                        Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(order.CompanyKey)
                            .CancelOrder(order.IBSOrderId.Value, currentIbsAccountId.Value, order.Settings.Phone);
                        cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
                    });
                }
            }
        }

        private bool IsValid(Address address)
        {
// ReSharper disable CompareOfFloatsByEqualityOperator
            return ((address != null) && address.FullAddress.HasValue() 
                                      && address.Longitude != 0 
                                      && address.Latitude != 0);
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private string BuildNote(string chargeType, string note, string buildingName, int largeBags, string promoCode)
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

            return FareHelper.GetFareFromAmountInclTax(estimate.Price.Value, 0);
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

        private BestAvailableCompany FindSpecificCompany(string market, CreateReportOrder createReportOrder, string orderCompanyKey = null, int? orderFleetId = null)
        {
            if (!orderCompanyKey.HasValue() && !orderFleetId.HasValue)
            {
                Exception createOrderException = new ArgumentNullException("You must at least provide a value for orderCompanyKey or orderFleetId");
				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
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

        private bool FetchCompanyPaymentSettings(string companyKey)
        {
            try
            {
                var paymentSettings = _serverSettings.GetPaymentSettings();
                var companyPaymentSettings = _taxiHailNetworkServiceClient.GetPaymentSettings(companyKey);

                // Mobile will always keep local settings. The only values that needs to be overridden are the payment providers settings.
                paymentSettings.Id = Guid.NewGuid();
                paymentSettings.CompanyKey = companyKey;
                paymentSettings.PaymentMode = companyPaymentSettings.PaymentMode;
                paymentSettings.BraintreeServerSettings = new BraintreeServerSettings
                {
                    IsSandbox = companyPaymentSettings.BraintreePaymentSettings.IsSandbox,
                    MerchantId = companyPaymentSettings.BraintreePaymentSettings.MerchantId,
                    PrivateKey = companyPaymentSettings.BraintreePaymentSettings.PrivateKey,
                    PublicKey = companyPaymentSettings.BraintreePaymentSettings.PublicKey
                };
                paymentSettings.BraintreeClientSettings = new BraintreeClientSettings
                {
                    ClientKey = companyPaymentSettings.BraintreePaymentSettings.ClientKey
                };
                paymentSettings.MonerisPaymentSettings = new MonerisPaymentSettings
                {
                    IsSandbox = companyPaymentSettings.MonerisPaymentSettings.IsSandbox,
                    ApiToken = companyPaymentSettings.MonerisPaymentSettings.ApiToken,
                    BaseHost = companyPaymentSettings.MonerisPaymentSettings.BaseHost,
                    SandboxHost = companyPaymentSettings.MonerisPaymentSettings.SandboxHost,
                    StoreId = companyPaymentSettings.MonerisPaymentSettings.StoreId
                };
                paymentSettings.CmtPaymentSettings = new CmtPaymentSettings
                {
                    BaseUrl = companyPaymentSettings.CmtPaymentSettings.BaseUrl,
                    ConsumerKey = companyPaymentSettings.CmtPaymentSettings.ConsumerKey,
                    ConsumerSecretKey = companyPaymentSettings.CmtPaymentSettings.ConsumerSecretKey,
                    CurrencyCode = companyPaymentSettings.CmtPaymentSettings.CurrencyCode,
                    FleetToken = companyPaymentSettings.CmtPaymentSettings.FleetToken,
                    IsManualRidelinqCheckInEnabled = companyPaymentSettings.CmtPaymentSettings.IsManualRidelinqCheckInEnabled,
                    IsSandbox = companyPaymentSettings.CmtPaymentSettings.IsSandbox,
                    Market = companyPaymentSettings.CmtPaymentSettings.Market,
                    MobileBaseUrl = companyPaymentSettings.CmtPaymentSettings.MobileBaseUrl,
                    SandboxBaseUrl = companyPaymentSettings.CmtPaymentSettings.SandboxBaseUrl,
                    SandboxMobileBaseUrl = companyPaymentSettings.CmtPaymentSettings.SandboxMobileBaseUrl
                };

                // Save/update company settings
                _commandBus.Send(new UpdatePaymentSettings
                {
                    ServerPaymentSettings = paymentSettings
                });

                return companyPaymentSettings.PaymentMode == PaymentMethod.Cmt
                    || companyPaymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("An error occurred when trying to get PaymentSettings for company {0}", companyKey));
                Log.Error(ex);

                return false;
            }
        }

		private ApplyPromotion ValidateAndApplyPromotion(string companyKey, string promoCode, int? chargeTypeId, Guid accountId, Guid orderId, DateTime pickupDate, bool isFutureBooking, string clientLanguageCode, CreateReportOrder createReportOrder)
        {
            if (!promoCode.HasValue())
            {
                // No promo code entered
                return null;
            }

            var usingPaymentInApp = chargeTypeId == ChargeTypes.CardOnFile.Id || chargeTypeId == ChargeTypes.PayPal.Id;
            if (!usingPaymentInApp)
            {
                var payPalIsEnabled = _serverSettings.GetPaymentSettings(companyKey).PayPalClientSettings.IsEnabled;
                var cardOnFileIsEnabled = _serverSettings.GetPaymentSettings(companyKey).IsPayInTaxiEnabled;

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
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get(promotionErrorResourceKey, clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
            }

            var promo = _promotionDao.FindByPromoCode(promoCode);
            if (promo == null)
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get("CannotCreateOrder_PromotionDoesNotExist", clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}

            var promoDomainObject = _promoRepository.Get(promo.Id);
            string errorMessage;
            if (!promoDomainObject.CanApply(accountId, pickupDate, isFutureBooking, out errorMessage))
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get(errorMessage, clientLanguageCode));

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}

            return new ApplyPromotion
            {
                PromoId = promo.Id,
                AccountId = accountId,
                OrderId = orderId,
                PickupDate = pickupDate,
                IsFutureBooking = isFutureBooking
            };
        }

        private string GetCreateOrderServiceErrorMessage(ErrorCode errorCode, string language)
        {
            var callMessage = string.Format(_resources.Get("ServiceError" + errorCode, language),
                _serverSettings.ServerData.TaxiHail.ApplicationName,
                _serverSettings.ServerData.DefaultPhoneNumberDisplay);

            var noCallMessage = _resources.Get("ServiceError" + errorCode + "_NoCall", language);

            return _serverSettings.ServerData.HideCallDispatchButton ? noCallMessage : callMessage;
        }

		private void CapturePaymentForPrepaidOrder(string companyKey, Guid orderId, AccountDetail account, decimal appEstimateWithTip, int tipPercentage, decimal bookingFees, string cvv, CreateReportOrder createReportOrder)
        {
            // Note: No promotion on web
            var tipAmount = FareHelper.GetTipAmountFromTotalIncludingTip(appEstimateWithTip, tipPercentage);
            var totalAmount = appEstimateWithTip + bookingFees;
            var meterAmount = appEstimateWithTip - tipAmount;

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, account, totalAmount, isForPrepaid: true, cvv: cvv);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    companyKey,
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
                    var paymentDetail = _orderPaymentDao.FindByOrderId(orderId, companyKey);

                    var fareObject = FareHelper.GetFareFromAmountInclTax(meterAmount,
                        _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(companyKey, orderId),
                        TotalAmount = totalAmount,
                        MeterAmount = fareObject.AmountExclTax,
                        TipAmount = tipAmount,
                        TaxAmount = fareObject.TaxAmount,
                        AuthorizationCode = commitResponse.AuthorizationCode,
                        TransactionId = commitResponse.TransactionId,
                        IsForPrepaidOrder = true,
                        BookingFees = bookingFees
                    });
                }
                else
                {
                    // Payment failed, void preauth
                    _paymentService.VoidPreAuthorization(companyKey, orderId, true);

                    Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), commitResponse.Message);

					createReportOrder.Error = createOrderException.ToString();
					_commandBus.Send(createReportOrder);
					throw createOrderException;
				}
            }
            else
            {
                Exception createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), preAuthResponse.Message);

				createReportOrder.Error = createOrderException.ToString();
				_commandBus.Send(createReportOrder);
				throw createOrderException;
			}
        }

        private CreateReportOrder CreateReportOrder(CreateOrder request, AccountDetail account)
        {
            return new CreateReportOrder
            {
                PickupDate = request.PickupDate ?? DateTime.Now,
                UserNote = request.Note,
                PickupAddress = request.PickupAddress,
                DropOffAddress = request.DropOffAddress,
                Settings = request.Settings,
                ClientLanguageCode = request.ClientLanguageCode,
                UserLatitude = request.UserLatitude,
                UserLongitude = request.UserLongitude,
                CompanyKey = request.OrderCompanyKey,
                AccountId = account.Id,
                OrderId = request.Id,
                EstimatedFare = request.Estimate.Price,
                UserAgent = Request.UserAgent,
                ClientVersion = Request.Headers.Get("ClientVersion")
            };
        }

        private class IBSOrderResult
        {
            public int? CreateOrderResult { get; set; }

            public OrderHailResult HailResult { get; set; }

            public bool IsHailRequest { get; set; }
        }

        private class BestAvailableCompany
        {
            public string CompanyKey { get; set; }

            public string CompanyName { get; set; }
        }

        private class ChargeAccountValidationResult
        {
            public string[] Prompts { get; set; }

            public int?[] PromptsLength { get; set; }

            public string ChargeTypeKeyOverride { get; set; }

            public bool IsChargeAccountPaymentWithCardOnFile { get; set; }
        }

        private class TemporaryOrderCreationInfo
        {
            public Guid OrderId { get; set; } 
            public AccountDetail Account { get; set; }
            public CreateOrder Request { get; set; } 
            public ReferenceData ReferenceData { get; set; }
            public string ChargeTypeIbs { get; set; } 
            public string ChargeTypeEmail { get; set; } 
            public string VehicleType { get; set; } 
            public string[] Prompts { get; set; } 
            public int?[] PromptsLength { get; set; } 
            public BestAvailableCompany BestAvailableCompany { get; set; }
            public ApplyPromotion ApplyPromoCommand { get; set; }
        }
    }
}