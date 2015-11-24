using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers.CreateOrder;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Services.OrderCreation
{
    public class CreateOrderService : BaseCreateOrderService
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
	    private readonly ILogger _logger;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly TaxiHailNetworkHelper _taxiHailNetworkHelper;
        private readonly Resources.Resources _resources;

        public CreateOrderService(ICommandBus commandBus,
            IAccountDao accountDao,
            IServerSettings serverSettings,
            ReferenceDataService referenceDataService,
            IIBSServiceProvider ibsServiceProvider,
            IRuleCalculator ruleCalculator,
            IAccountChargeDao accountChargeDao,
            ICreditCardDao creditCardDao,
            IOrderDao orderDao,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IPaymentService paymentService,
            IPayPalServiceFactory payPalServiceFactory,
            IOrderPaymentDao orderPaymentDao,
            IFeesDao feesDao, 
            ILogger logger,
            IIbsCreateOrderService ibsCreateOrderService,
            IDispatcherService dispatcherService)
            : base(serverSettings, commandBus, accountChargeDao, paymentService, creditCardDao,
                   ibsServiceProvider, promotionDao, promoRepository, orderPaymentDao, accountDao,
                   payPalServiceFactory, logger, taxiHailNetworkServiceClient, ruleCalculator,
                   feesDao, referenceDataService, orderDao, dispatcherService)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
	        _logger = logger;
            _ibsCreateOrderService = ibsCreateOrderService;
            _resources = new Resources.Resources(_serverSettings);

            _taxiHailNetworkHelper = new TaxiHailNetworkHelper(_accountDao, ibsServiceProvider, _serverSettings, _taxiHailNetworkServiceClient, _commandBus, _logger);
        }

        public object Post(CreateOrderRequest request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var createReportOrder = CreateReportOrder(request, account);

            var createOrderCommand = BuildCreateOrderCommand(request, account, createReportOrder);

            // Initialize PayPal if user is using PayPal web
            var paypalWebPaymentResponse = PaymentHelper.InitializePayPalCheckoutIfNecessary(
                createOrderCommand.AccountId, createOrderCommand.IsPrepaid, createOrderCommand.OrderId,
                request, createOrderCommand.BookingFees, createOrderCommand.CompanyKey, createReportOrder, Request.AbsoluteUri);

            _commandBus.Send(createOrderCommand);

            if (paypalWebPaymentResponse != null)
            {
                // Order prepaid by PayPal

                _commandBus.Send(new SaveTemporaryOrderCreationInfo
                {
                    OrderId = createOrderCommand.OrderId,
                    SerializedOrderCreationInfo = new TemporaryOrderCreationInfo
                    {
                        OrderId = createOrderCommand.OrderId,
                        AccountId = createOrderCommand.AccountId,
                        Request = createOrderCommand,
                        ReferenceDataCompaniesList = createOrderCommand.ReferenceDataCompanyList,
                        ChargeTypeIbs = createOrderCommand.Settings.ChargeType,
                        ChargeTypeEmail = createOrderCommand.ChargeTypeEmail,
                        VehicleType = createOrderCommand.Settings.VehicleType,
                        Prompts = createOrderCommand.Prompts,
                        PromptsLength = createOrderCommand.PromptsLength,
                        BestAvailableCompany = new BestAvailableCompany
                        {
                            CompanyKey = createOrderCommand.CompanyKey,
                            CompanyName = createOrderCommand.CompanyName
                        },
                        PromotionId = createOrderCommand.PromotionId
                    }.ToJson()
                });

                return paypalWebPaymentResponse;
            }

            if (request.QuestionsAndAnswers.HasValue())
            {
                // Save question answers so we can display them the next time the user books
                var accountLastAnswers = request.QuestionsAndAnswers
                    .Where(q => q.SaveAnswer)
                    .Select(q =>
                        new AccountChargeQuestionAnswer
                        {
                            AccountId = createOrderCommand.AccountId,
                            AccountChargeQuestionId = q.Id,
                            AccountChargeId = q.AccountId,
                            LastAnswer = q.Answer
                        });

                _commandBus.Send(new AddUpdateAccountQuestionAnswer { AccountId = createOrderCommand.AccountId, Answers = accountLastAnswers.ToArray() });
            }

            return new OrderStatusDetail
            {
                OrderId = createOrderCommand.OrderId,
                Status = OrderStatus.Created,
                IBSStatusId = string.Empty,
                IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", createOrderCommand.ClientLanguageCode),
            };
        }

        public object Post(SwitchOrderToNextDispatchCompanyRequest request)
        {
            _logger.LogMessage("Switching order to another IBS : " + request.ToJson());

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var order = _orderDao.FindById(request.OrderId);
            var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);

            if (orderStatusDetail.Status != OrderStatus.TimedOut)
            {
                // Only switch companies if order is timedout
                return orderStatusDetail;
            }

			// We are in a network timeout situation.
	        if (orderStatusDetail.CompanyKey == request.NextDispatchCompanyKey)
	        {
                _ibsCreateOrderService.CancelIbsOrder(order.IBSOrderId, order.CompanyKey, order.Settings.Phone, account.Id);

		        orderStatusDetail.IBSStatusId = VehicleStatuses.Common.Timeout;
		        orderStatusDetail.IBSStatusDescription = _resources.Get("OrderStatus_" + VehicleStatuses.Common.Timeout);
		        return orderStatusDetail;
	        }

            var market = _taxiHailNetworkServiceClient.GetCompanyMarket(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

            var isConfiguredForCmtPayment = _taxiHailNetworkHelper.FetchAndSaveNetworkPaymentSettings(request.NextDispatchCompanyKey);

            var chargeTypeId = order.Settings.ChargeTypeId;
            var chargeTypeDisplay = order.Settings.ChargeType;
            if (!isConfiguredForCmtPayment)
            {
                // Only companies configured for CMT payment can support CoF orders outside of home market
                chargeTypeId = ChargeTypes.PaymentInCar.Id;
                chargeTypeDisplay = ChargeTypes.PaymentInCar.Display;
            }

            var newOrderRequest = new CreateOrderRequest
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
                ClientLanguageCode = account.Language
            };

            var fare = FareHelper.GetFareFromEstimate(new RideEstimate { Price = order.EstimatedFare });
            var newReferenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = request.NextDispatchCompanyKey });

            // This must be localized with the priceformat to be localized in the language of the company
            // because it is sent to the driver
            var chargeTypeIbs = _resources.Get(chargeTypeDisplay, _serverSettings.ServerData.PriceFormat);

            var ibsInformationNote = IbsHelper.BuildNote(
                _serverSettings.ServerData.IBS.NoteTemplate,
                chargeTypeIbs,
                order.UserNote,
                order.PickupAddress.BuildingName,
                newOrderRequest.Settings.LargeBags,
                _serverSettings.ServerData.IBS.HideChargeTypeInUserNote);

            var networkErrorMessage = string.Format(_resources.Get("Network_CannotCreateOrder", order.ClientLanguageCode), request.NextDispatchCompanyName);

            int ibsAccountId;
            try
            {
                // Recreate order on next dispatch company IBS
                ibsAccountId = _taxiHailNetworkHelper.CreateIbsAccountIfNeeded(account, request.NextDispatchCompanyKey);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(networkErrorMessage);
                _logger.LogError(ex);

                throw new HttpError(HttpStatusCode.InternalServerError, networkErrorMessage);
            }

            ValidateProvider(newOrderRequest, newReferenceData, market.HasValue(), null);

            var newOrderCommand = Mapper.Map<CreateOrder>(newOrderRequest);
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
    }
}