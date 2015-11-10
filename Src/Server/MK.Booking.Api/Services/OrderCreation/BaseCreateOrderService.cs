﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers.CreateOrder;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Helpers;
using AutoMapper;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Services.OrderCreation
{
    public abstract class BaseCreateOrderService : Service
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly IPaymentService _paymentService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;
        private readonly TaxiHailNetworkHelper _taxiHailNetworkHelper;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IFeesDao _feesDao;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IOrderDao _orderDao;
        private readonly IDispatcherService _dispatcherService;

        private readonly Resources.Resources _resources;

        protected readonly CreateOrderPaymentHelper PaymentHelper;

        internal BaseCreateOrderService(IServerSettings serverSettings,
            ICommandBus commandBus,
            IAccountChargeDao accountChargeDao,
            IPaymentService paymentService,
            ICreditCardDao creditCardDao,
            IIBSServiceProvider ibsServiceProvider,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            IOrderPaymentDao orderPaymentDao,
            IAccountDao accountDao,
            IPayPalServiceFactory payPalServiceFactory,
            ILogger logger,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IRuleCalculator ruleCalculator,
            IFeesDao feesDao,
            ReferenceDataService referenceDataService,
            IOrderDao orderDao,
            IDispatcherService dispatcherService)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountChargeDao = accountChargeDao;
            _paymentService = paymentService;
            _creditCardDao = creditCardDao;
            _ibsServiceProvider = ibsServiceProvider;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _accountDao = accountDao;
            _logger = logger;
            _ruleCalculator = ruleCalculator;
            _feesDao = feesDao;
            _referenceDataService = referenceDataService;
            _orderDao = orderDao;
            _dispatcherService = dispatcherService;

            _resources = new Resources.Resources(_serverSettings);
            _taxiHailNetworkHelper = new TaxiHailNetworkHelper(_serverSettings, taxiHailNetworkServiceClient, _commandBus, _logger);

            PaymentHelper = new CreateOrderPaymentHelper(serverSettings, commandBus, paymentService, orderPaymentDao, payPalServiceFactory);
        }

        protected CreateOrder BuildCreateOrderCommand(CreateOrderRequest request, AccountDetail account, CreateReportOrder createReportOrder)
        {
            _logger.LogMessage("Create order request : " + request.ToJson());

            var countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Settings.Country));

            if (PhoneHelper.IsPossibleNumber(countryCode, request.Settings.Phone))
            {
                request.Settings.Phone = PhoneHelper.GetDigitsFromPhoneNumber(request.Settings.Phone);
            }
            else
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
                    string.Format(_resources.Get("PhoneNumberFormat", request.ClientLanguageCode), countryCode.GetPhoneExample()));
            }

            // TODO: Find a better way to do this...
            var isFromWebApp = request.FromWebApp;

            if (!isFromWebApp)
            {
                ValidateAppVersion(request.ClientLanguageCode, createReportOrder);
            }

            // Find market
            var market = _dispatcherService.GetSettings(request.PickupAddress.Latitude, request.PickupAddress.Longitude).Market;

            createReportOrder.Market = market;

            BestAvailableCompany bestAvailableCompany;

            if (request.OrderCompanyKey.HasValue() || request.OrderFleetId.HasValue)
            {
                // For API user, it's possible to manually specify which company to dispatch to by using a fleet id
                bestAvailableCompany = _taxiHailNetworkHelper.FindSpecificCompany(market, createReportOrder, request.OrderCompanyKey, request.OrderFleetId);
            }
            else
            {
                bestAvailableCompany = _taxiHailNetworkHelper.FindBestAvailableCompany(market, request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }

            createReportOrder.CompanyKey = bestAvailableCompany.CompanyKey;
            createReportOrder.CompanyName = bestAvailableCompany.CompanyName;

            if (market.HasValue() && !bestAvailableCompany.CompanyKey.HasValue())
            {
                // No companies available that are desserving this region for the company
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_NoCompanies", request.ClientLanguageCode));
            }
            
            if (market.HasValue())
            {
                _taxiHailNetworkHelper.UpdateVehicleTypeFromMarketData(request.Settings, bestAvailableCompany.CompanyKey);
                var isConfiguredForCmtPayment = _taxiHailNetworkHelper.FetchCompanyPaymentSettings(bestAvailableCompany.CompanyKey);

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
                    ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_PendingOrder, pendingOrderId.ToString());
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
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, rule.Message);
            }

            // We need to validate the rules of the roaming market.
            if (market.HasValue())
            {
                // External market, query company site directly to validate their rules
                var orderServiceClient = new RoamingValidationServiceClient(bestAvailableCompany.CompanyKey, _serverSettings.ServerData.Target);

                _logger.LogMessage(string.Format("Validating rules for company in external market... Target: {0}, Server: {1}", _serverSettings.ServerData.Target, orderServiceClient.Url));

                var validationResult = orderServiceClient.ValidateOrder(request, true);
                if (validationResult.HasError)
                {
                    ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, validationResult.Message);
                }
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_SettingsRequired);
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
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_NoFareEstimateAvailable,
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_NoFareEstimateAvailable, request.ClientLanguageCode));
            }

            // IBS provider validation
            ValidateProvider(request, referenceData, market.HasValue(), createReportOrder);

            // Map the command to obtain a OrderId (web doesn't prepopulate it in the request)
            var orderCommand = Mapper.Map<Commands.CreateOrder>(request);

            var marketFees = _feesDao.GetMarketFees(market);
            orderCommand.BookingFees = marketFees != null ? marketFees.Booking : 0;
            createReportOrder.BookingFees = orderCommand.BookingFees;

            // Promo code validation
            var promotionId = ValidatePromotion(bestAvailableCompany.CompanyKey, request.PromoCode, request.Settings.ChargeTypeId, account.Id, pickupDate, isFutureBooking, request.ClientLanguageCode, createReportOrder);

            // Charge account validation
            var accountValidationResult = ValidateChargeAccountIfNecessary(bestAvailableCompany.CompanyKey, request, orderCommand.OrderId, account, isFutureBooking, isFromWebApp, orderCommand.BookingFees, createReportOrder);
            createReportOrder.IsChargeAccountPaymentWithCardOnFile = accountValidationResult.IsChargeAccountPaymentWithCardOnFile;

            // if ChargeAccount uses payment with card on file, payment validation was already done
            if (!accountValidationResult.IsChargeAccountPaymentWithCardOnFile)
            {
                // Payment method validation
                ValidatePayment(bestAvailableCompany.CompanyKey, request, orderCommand.OrderId, account, isFutureBooking, request.Estimate.Price, orderCommand.BookingFees, isPrepaid, createReportOrder);
            }

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

            var ibsInformationNote = IbsNoteBuilder.BuildNote(
                _serverSettings.ServerData.IBS.NoteTemplate,
                chargeTypeIbs,
                request.Note,
                request.PickupAddress.BuildingName,
                request.Settings.LargeBags,
                _serverSettings.ServerData.IBS.HideChargeTypeInUserNote);

            var fare = FareHelper.GetFareFromEstimate(request.Estimate);

            orderCommand.AccountId = account.Id;
            orderCommand.UserAgent = Request.UserAgent;
            orderCommand.ClientVersion = Request.Headers.Get("ClientVersion");
            orderCommand.IsChargeAccountPaymentWithCardOnFile = accountValidationResult.IsChargeAccountPaymentWithCardOnFile;
            orderCommand.CompanyKey = bestAvailableCompany.CompanyKey;
            orderCommand.CompanyName = bestAvailableCompany.CompanyName;
            orderCommand.CompanyFleetId = bestAvailableCompany.FleetId;
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
            orderCommand.ChargeTypeEmail = chargeTypeEmail;

            Debug.Assert(request.PickupDate != null, "request.PickupDate != null");

            return orderCommand;
        }

        protected void ValidateProvider(CreateOrderRequest request, ReferenceData referenceData, bool isInExternalMarket, CreateReportOrder createReportOrder)
        {
            // Provider is optional for home market
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (!isInExternalMarket
                && request.Settings.ProviderId.HasValue
                && referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_InvalidProvider,
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_InvalidProvider, request.ClientLanguageCode));
            }
        }

        protected DateTime GetCurrentOffsetedTime(string companyKey)
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

        protected int CreateIbsAccountIfNeeded(AccountDetail account, string companyKey = null)
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

        protected CreateReportOrder CreateReportOrder(CreateOrderRequest request, AccountDetail account)
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
                ClientVersion = Request.Headers.Get("ClientVersion"),
                TipIncentive = request.TipIncentive
            };
        }

        private void ValidateAppVersion(string clientLanguage, CreateReportOrder createReportOrder)
        {
            var appVersion = base.Request.Headers.Get("ClientVersion");
            var minimumAppVersion = _serverSettings.ServerData.MinimumRequiredAppVersion;

            if (appVersion.IsNullOrEmpty() || minimumAppVersion.IsNullOrEmpty())
            {
                return;
            }

            var minimumMajorMinorBuild = minimumAppVersion.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            var appMajorMinorBuild = appVersion.Split('.');

            for (var i = 0; i < appMajorMinorBuild.Length; i++)
            {
                var appVersionItem = int.Parse(appMajorMinorBuild[i]);
                var minimumVersionItem = int.Parse(minimumMajorMinorBuild.Length <= i ? "0" : minimumMajorMinorBuild[i]);

                if (appVersionItem < minimumVersionItem)
                {
                    ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
                        _resources.Get("CannotCreateOrderInvalidVersion", clientLanguage));
                }
            }
        }

        private ChargeAccountValidationResult ValidateChargeAccountIfNecessary(string companyKey, CreateOrderRequest request, Guid orderId, AccountDetail account, bool isFutureBooking, bool isFromWebApp, decimal bookingFees, CreateReportOrder createReportOrder)
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
                        ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
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

        private void ValidateChargeAccountAnswers(string accountNumber, string customerNumber,
            IEnumerable<AccountChargeQuestion> userQuestionsDetails, string clientLanguageCode, CreateReportOrder createReportOrder)
        {
            var accountChargeDetail = _accountChargeDao.FindByAccountNumber(accountNumber);
            if (accountChargeDetail == null)
            {
                ThrowAndLogException(createReportOrder, ErrorCode.AccountCharge_InvalidAccountNumber,
                    GetCreateOrderServiceErrorMessage(ErrorCode.AccountCharge_InvalidAccountNumber, clientLanguageCode));
            }

            var answers = userQuestionsDetails.Select(x => x.Answer);

            var validation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(answers, accountNumber, customerNumber);
            if (!validation.Valid)
            {
                if (validation.ValidResponse != null)
                {
                    int firstError = validation.ValidResponse.IndexOf(false);
                    string errorMessage = null;
                    if (accountChargeDetail != null)
                    {
                        errorMessage = accountChargeDetail.Questions[firstError].ErrorMessage;
                    }

                    ThrowAndLogException(createReportOrder, ErrorCode.AccountCharge_InvalidAnswer, errorMessage);
                }

                ThrowAndLogException(createReportOrder, ErrorCode.AccountCharge_InvalidAccountNumber, validation.Message);
            }
        }

        private void ValidatePayment(string companyKey, CreateOrderRequest request, Guid orderId, AccountDetail account, bool isFutureBooking, double? appEstimate, decimal bookingFees, bool isPrepaid, CreateReportOrder createReportOrder)
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
                    ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
                        _resources.Get("CannotCreateOrder_PrepaidButPrepaidNotEnabled", request.ClientLanguageCode));
                }

                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    if (!appEstimateWithTip.HasValue)
                    {
                        ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
                            _resources.Get("CannotCreateOrder_PrepaidNoEstimate", request.ClientLanguageCode));
                    }

                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv, createReportOrder);

                    var result = PaymentHelper.CapturePaymentForPrepaidOrder(companyKey, orderId, account, Convert.ToDecimal(appEstimateWithTip), tipPercent, bookingFees, request.Cvv, createReportOrder);
                    if (!result.IsSuccessful)
                    {
                        ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, result.Message);
                    }
                }
            }
            else
            {
                // Payment mode is CardOnFile
                if (request.Settings.ChargeTypeId.HasValue
                    && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
                {
                    ValidateCreditCard(account, request.ClientLanguageCode, request.Cvv, createReportOrder);

                    var isSuccessful = PaymentHelper.PreAuthorizePaymentMethod(companyKey, orderId, account,
                        request.ClientLanguageCode, isFutureBooking, appEstimateWithTip, bookingFees,
                        false, createReportOrder, request.Cvv);

                    if (!isSuccessful)
                    {
                        ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable,
                            _resources.Get("CannotCreateOrder_CreditCardWasDeclined", request.ClientLanguageCode));
                    }
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
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_CardOnFileButNoCreditCard,
                    GetCreateOrderServiceErrorMessage(ErrorCode.CreateOrder_CardOnFileButNoCreditCard, clientLanguageCode));
            }

            var creditCard = _creditCardDao.FindByAccountId(account.Id).First();
            if (creditCard.IsExpired())
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_CreditCardExpired", clientLanguageCode));
            }
            if (creditCard.IsDeactivated)
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_CardOnFileDeactivated, _resources.Get("CannotCreateOrder_CreditCardDeactivated", clientLanguageCode));
            }

            if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking
                && !cvv.HasValue())
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_CreditCardCvvRequired", clientLanguageCode));
            }
        }

        private void ValidatePayPal(string companyKey, Guid orderId, AccountDetail account, string clientLanguageCode, bool isFutureBooking, decimal? appEstimateWithTip, decimal bookingFees, CreateReportOrder createReportOrder)
        {
            if (!_serverSettings.GetPaymentSettings(companyKey).PayPalClientSettings.IsEnabled
                    || !account.IsPayPalAccountLinked)
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_PayPalButNoPayPal", clientLanguageCode));
            }

            var isSuccessful = PaymentHelper.PreAuthorizePaymentMethod(companyKey, orderId, account, clientLanguageCode, isFutureBooking, appEstimateWithTip, bookingFees, true, createReportOrder);
            if (!isSuccessful)
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_PayPalWasDeclined", clientLanguageCode));
            }
        }

        private Guid? ValidatePromotion(string companyKey, string promoCode, int? chargeTypeId, Guid accountId, DateTime pickupDate, bool isFutureBooking, string clientLanguageCode, CreateReportOrder createReportOrder)
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
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get(promotionErrorResourceKey, clientLanguageCode));
            }

            var promo = _promotionDao.FindByPromoCode(promoCode);
            if (promo == null)
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get("CannotCreateOrder_PromotionDoesNotExist", clientLanguageCode));
            }

            var promoDomainObject = _promoRepository.Get(promo.Id);
            string errorMessage;
            if (!promoDomainObject.CanApply(accountId, pickupDate, isFutureBooking, out errorMessage))
            {
                ThrowAndLogException(createReportOrder, ErrorCode.CreateOrder_RuleDisable, _resources.Get(errorMessage, clientLanguageCode));
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

        private void ThrowAndLogException(CreateReportOrder createReportOrder, ErrorCode errorCodeType, string errorMessage = null)
        {
            var createOrderException = new HttpError(HttpStatusCode.BadRequest, errorCodeType.ToString(), errorMessage);

            createReportOrder.Error = createOrderException.ToString();
            _commandBus.Send(createReportOrder);

            throw createOrderException;
        }
    }
}