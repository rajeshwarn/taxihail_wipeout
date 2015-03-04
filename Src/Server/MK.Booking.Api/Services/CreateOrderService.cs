#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
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
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using AutoMapper;
using CustomerPortal.Client;
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
            IPaymentService paymentService)
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

            _resources = new Resources.Resources(_serverSettings);
        }

        public object Post(CreateOrder request)
        {
            Log.Info("Create order request : " + request.ToJson());

            if (!request.FromWebApp)
            {
                ValidateAppVersion(request.ClientLanguageCode);
            }

            if (request.Market.HasValue())
            {
                // Only pay in car charge type supported for orders outside home market
                request.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
            }
            else
            {
                // Ensure that the market is not an empty string
                request.Market = null;
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            BestAvailableCompany bestAvailableCompany;

            if (request.OrderCompanyKey.HasValue() || request.OrderFleetId.HasValue)
            {
                // For API user, it's possible to manually specify which company to dispatch to
                bestAvailableCompany = FindSpecificCompany(request.Market, request.OrderCompanyKey, request.OrderFleetId);
            }
            else
            {
                bestAvailableCompany = FindBestAvailableCompany(request.Market, request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }

            if (request.Market.HasValue() && bestAvailableCompany.CompanyKey == null)
            {
                // No companies available that are desserving this region for the company
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrder_NoCompanies", request.ClientLanguageCode));
            }

            account.IBSAccountId = CreateIbsAccountIfNeeded(account, bestAvailableCompany.CompanyKey, request.Market);
            
            var isFutureBooking = request.PickupDate.HasValue;
            var pickupDate = request.PickupDate ?? GetCurrentOffsetedTime(bestAvailableCompany.CompanyKey, request.Market);

            // User can still create future order, but we allow only one active Book now order.
            if (!isFutureBooking)
            {
                var pendingOrderId = GetPendingOrder();

                // We don't allow order creation if there's already an order scheduled
                if (!_serverSettings.ServerData.AllowSimultaneousAppOrders
                    && pendingOrderId != null
                    && !request.FromWebApp)
                {
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_PendingOrder.ToString(), pendingOrderId.ToString());
                }
            }

            // We can only validate rules when in the local market
            if (!request.Market.HasValue())
            {
                var rule = _ruleCalculator.GetActiveDisableFor(
                    isFutureBooking,
                    pickupDate,
                    () =>
                        _ibsServiceProvider.StaticData(bestAvailableCompany.CompanyKey, request.Market)
                            .GetZoneByCoordinate(
                                request.Settings.ProviderId,
                                request.PickupAddress.Latitude,
                                request.PickupAddress.Longitude),
                    () => request.DropOffAddress != null
                        ? _ibsServiceProvider.StaticData(bestAvailableCompany.CompanyKey, request.Market)
                            .GetZoneByCoordinate(
                                request.Settings.ProviderId,
                                request.DropOffAddress.Latitude,
                                request.DropOffAddress.Longitude)
                            : null);

                if (rule != null)
                {
                    throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);
                }
            }
            else
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

            if (request.Market.HasValue())
            {
                referenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = bestAvailableCompany.CompanyKey, Market = request.Market });
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
            ValidateProvider(request, referenceData);

            // Promo code validation
            var applyPromoCommand = ValidateAndApplyPromotion(request.PromoCode, request.Settings.ChargeTypeId, account.Id, request.Id, pickupDate, isFutureBooking, request.ClientLanguageCode);
            
            // Payment method validation
            ValidatePayment(request, account, isFutureBooking, request.Estimate.Price);

            // Charge account validation
            var accountValidationResult = ValidateChargeAccount(request, account, isFutureBooking);

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

            var orderCommand = Mapper.Map<Commands.CreateOrder>(request);
            orderCommand.AccountId = account.Id;
            orderCommand.UserAgent = base.Request.UserAgent;
            orderCommand.ClientVersion = base.Request.Headers.Get("ClientVersion");
            orderCommand.IsChargeAccountPaymentWithCardOnFile = accountValidationResult.IsChargeAccountPaymentWithCardOnFile;
            orderCommand.CompanyKey = bestAvailableCompany.CompanyKey;
            orderCommand.CompanyName = bestAvailableCompany.CompanyName;
            orderCommand.Market = request.Market;
            orderCommand.Settings.ChargeType = chargeTypeIbs;
            orderCommand.Settings.VehicleType = vehicleType;
            _commandBus.Send(orderCommand);

            // Create order on IBS
            Task.Run(() => 
                CreateOrderOnIBSAndSendCommands(orderCommand.OrderId, account,
                    request, referenceData, chargeTypeIbs, chargeTypeEmail, vehicleType,
                    accountValidationResult.Prompts, accountValidationResult.PromptsLength,
                    bestAvailableCompany, applyPromoCommand));
            
            return new OrderStatusDetail
            {
                OrderId = orderCommand.OrderId,
                Status = OrderStatus.Created,
                IBSStatusId = string.Empty,
                IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", orderCommand.ClientLanguageCode),
            };
        }

        private ChargeAccountValidationResult ValidateChargeAccount(CreateOrder request, AccountDetail account, bool isFutureBooking)
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
                    if (request.FromWebApp || request.Market.HasValue())
                    {
                        // Card on file payment not supported by the web app and when not in home market
                        throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrderChargeAccountNotSupported", request.ClientLanguageCode));
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

                    ValidatePayment(request, account, isFutureBooking, request.Estimate.Price);

                    isChargeAccountPaymentWithCardOnFile = true;
                }

                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.QuestionsAndAnswers, request.ClientLanguageCode);

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

        private async void CreateOrderOnIBSAndSendCommands(Guid orderId, AccountDetail account, CreateOrder request, ReferenceData referenceData, 
            string chargeTypeIbs, string chargeTypeEmail, string vehicleType, string[] prompts, int?[] promptsLength, BestAvailableCompany bestAvailableCompany, 
            ApplyPromotion applyPromoCommand)
        {
            var ibsOrderId = CreateIbsOrder(account.IBSAccountId.Value, request, referenceData, chargeTypeIbs, prompts, promptsLength, bestAvailableCompany.CompanyKey);

            // Wait for order creation to complete before sending other commands
            await Task.Delay(750);

            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? "" : "_" + Math.Abs(ibsOrderId.Value);
                var errorCode = ErrorCode.CreateOrder_CannotCreateInIbs + code;

                var errorCommand = new CancelOrderBecauseOfIbsError
                {
                    OrderId = orderId,
                    ErrorCode = errorCode,
                    ErrorDescription = _resources.Get(errorCode, request.ClientLanguageCode)
                };
                _commandBus.Send(errorCommand);
            }
            else
            {
                var ibsCommand = new AddIbsOrderInfoToOrder
                {
                    OrderId = orderId,
                    IBSOrderId = ibsOrderId.Value
                };
                _commandBus.Send(ibsCommand);

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

        private void ValidateProvider(CreateOrder request, ReferenceData referenceData)
        {
            // Provider is optional for home market
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (request.Settings.ProviderId.HasValue &&
                !request.Market.HasValue() &&
                referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_InvalidProvider.ToString(), 
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_InvalidProvider, request.ClientLanguageCode));
            }
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

            var newOrderRequest = new CreateOrder
            {
                PickupDate = GetCurrentOffsetedTime(request.NextDispatchCompanyKey, order.Market),
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.DropOffAddress,
                Market = order.Market,
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
                Estimate = new CreateOrder.RideEstimate { Price = order.EstimatedFare }
            };

            var newReferenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = request.NextDispatchCompanyKey, Market = order.Market });

            // This must be localized with the priceformat to be localized in the language of the company
            // because it is sent to the driver
            var chargeTypeIbs = _resources.Get(ChargeTypes.PaymentInCar.Display, _serverSettings.ServerData.PriceFormat);

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

            ValidateProvider(newOrderRequest, newReferenceData);

            var newIbsOrderId = CreateIbsOrder(ibsAccountId, newOrderRequest, newReferenceData, chargeTypeIbs, null, null, request.NextDispatchCompanyKey);
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
                Market = order.Market
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

        private int CreateIbsAccountIfNeeded(AccountDetail account, string companyKey = null, string market = null)
        {
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, companyKey);
            if (ibsAccountId.HasValue)
            {
                return ibsAccountId.Value;
            }

            // Account doesn't exist, create it
            ibsAccountId = _ibsServiceProvider.Account(companyKey, market).CreateAccount(account.Id,
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

        private void ValidatePayment(CreateOrder request, AccountDetail account, bool isFutureBooking, double? appEstimate)
        {
            var tipPercent = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;

            // If there's an estimate, add tip to that estimate
            if (appEstimate.HasValue)
            {
                appEstimate = GetTipAmount(appEstimate.Value, tipPercent);
            }

            var appEstimateWithTip = appEstimate.HasValue ? Convert.ToDecimal(appEstimate.Value) : (decimal?)null;

            // Payment mode is CardOnFile
            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
            {
                ValidateCreditCard(request.Id, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip);
            }

            // Payment mode is PayPal
            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.PayPal.Id)
            {
                ValidatePayPal(request.Id, account, request.ClientLanguageCode, isFutureBooking, appEstimateWithTip);
            }
        }

        private double GetTipAmount(double amount, double percentage)
        {
            var tip = percentage / 100;
            return Math.Round(amount * tip, 2);
        }

        private void ValidateCreditCard(Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimate)
        {
            // check if the account has a credit card
            if (!account.DefaultCreditCard.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileButNoCreditCard.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_CardOnFileButNoCreditCard, clientLanguageCode));
            }

            var creditCard = _creditCardDao.FindByAccountId(account.Id).First();
            if (creditCard.IsDeactivated)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.CreateOrder_CardOnFileDeactivated.ToString(),
                    _resources.Get("CannotCreateOrder_CreditCardDeactivated", clientLanguageCode));
            }

            PreAuthorizePaymentMethod(orderId, account, clientLanguageCode, isFutureBooking, appEstimate, false);
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

        private void PreAuthorizePaymentMethod(Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimate, bool isPayPal)
        {
            if (!_serverSettings.GetPaymentSettings().IsPreAuthEnabled || isFutureBooking)
            {
                return;
            }
            
            // there's a minimum amount of $50 (warning indicating that on the admin ui)
            // if app returned an estimate, use it, otherwise use the setting (or 0), then use max between the value and 50
            var preAuthAmount = Math.Max(appEstimate ?? (_serverSettings.GetPaymentSettings().PreAuthAmount ?? 0), 50);
            
            var preAuthResponse = _paymentService.PreAuthorize(orderId, account, preAuthAmount);

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

        private void ValidateChargeAccountAnswers(string accountNumber, AccountChargeQuestion[] userQuestionsDetails, string clientLanguageCode)
        {
            var accountChargeDetail = _accountChargeDao.FindByAccountNumber(accountNumber);
            if (accountChargeDetail == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest,
                    ErrorCode.AccountCharge_InvalidAccountNumber.ToString(),
                    GetCreateOrderServiceErrorMessage(ErrorCode.AccountCharge_InvalidAccountNumber, clientLanguageCode));
            }

            var answers = userQuestionsDetails.Select(x => x.Answer);

            var validation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(answers, accountNumber, "0");
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

        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(() =>
            {
                _updateOrderStatusJob.CheckStatus(orderId);
            });
        }

        private DateTime GetCurrentOffsetedTime(string companyKey, string market)
        {
            //TODO : need to check ibs setup for shortesst time

            var ibsServerTimeDifference = _ibsServiceProvider.GetSettingContainer(companyKey, market).TimeDifference;
            var offsetedTime = DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }

        private int? CreateIbsOrder(int ibsAccountId, CreateOrder request, ReferenceData referenceData, string chargeType, string[] prompts, int?[] promptsLength, string companyKey = null)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                return new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000);
            }

            var defaultCompany = referenceData.CompaniesList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceData.CompaniesList.FirstOrDefault();

            var providerId = request.Market.HasValue() && referenceData.CompaniesList.Any() && defaultCompany != null
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
            int? ibsChargeTypeId;
            if (request.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id)
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

            var result = _ibsServiceProvider.Booking(companyKey, request.Market).CreateOrder(
                providerId,
                ibsAccountId,
                request.Settings.Name,
                request.Settings.Phone,
                request.Settings.Passengers,
                request.Settings.VehicleTypeId,
                ibsChargeTypeId,                    
                note,
                request.PickupDate.Value,
                ibsPickupAddress,
                ibsDropOffAddress,
                request.Settings.AccountNumber,
                string.IsNullOrWhiteSpace (request.Settings.AccountNumber ) ?(int?) null:0,
                prompts,
                promptsLength,
                fare);

            return result;
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
                        Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(order.CompanyKey, order.Market).CancelOrder(order.IBSOrderId.Value, currentIbsAccountId.Value, order.Settings.Phone);
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

            var promoCodeString = string.Empty;
            if (promoCode.HasValue())
            {
                var promo = _promotionDao.FindByPromoCode(promoCode);
                promoCodeString = promo.GetNoteToDriverFormattedString();
            }

            if (!string.IsNullOrWhiteSpace(noteTemplate))
            {
                noteTemplate = string.Format("{0}{1}{2}", 
                    chargeType,
                    promoCodeString.HasValue()
                        ? string.Format("{0}{1}{2}", Environment.NewLine, promoCodeString, Environment.NewLine)
                        : Environment.NewLine,
                    noteTemplate);

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
                    promoCodeString.HasValue()
                        ? string.Format("{0}{1}{2}", Environment.NewLine, promoCodeString, Environment.NewLine)
                        : Environment.NewLine, 
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

            return Fare.FromAmountInclTax(estimate.Price.Value, 0);
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

        private ApplyPromotion ValidateAndApplyPromotion(string promoCode, int? chargeTypeId, Guid accountId, Guid orderId, DateTime pickupDate, bool isFutureBooking, string clientLanguageCode)
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
    }
}