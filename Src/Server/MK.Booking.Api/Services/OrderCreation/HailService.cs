using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Client;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using AutoMapper;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Services.OrderCreation
{
    public class HailService : BaseCreateOrderService
    {
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;  
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly IDispatcherService _dispatcherService;
        private readonly Resources.Resources _resources;

        public HailService(
            IServerSettings serverSettings,
            ICommandBus commandBus,
            ILogger logger,
            IAccountChargeDao accountChargeDao,
            ICreditCardDao creditCardDao,
            IIBSServiceProvider ibsServiceProvider,
            IPromotionDao promotionDao,
            IAccountDao accountDao,
            IFeesDao feesDao,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPaymentService paymentService,
            ReferenceDataService referenceDataService,
            IIbsCreateOrderService ibsCreateOrderService,
            IEventSourcedRepository<Promotion> promoRepository,
            IPayPalServiceFactory payPalServiceFactory,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IRuleCalculator ruleCalculator,
            IDispatcherService dispatcherService,
            Resources.Resources resources)
            : base(serverSettings, commandBus, accountChargeDao, paymentService, creditCardDao,
                  ibsServiceProvider, promotionDao, promoRepository, orderPaymentDao, accountDao,
                  payPalServiceFactory, logger, taxiHailNetworkServiceClient, ruleCalculator,
                  feesDao, referenceDataService, orderDao)
        {
            _orderDao = orderDao;
            _logger = logger;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _ibsCreateOrderService = ibsCreateOrderService;
            _dispatcherService = dispatcherService;
            _resources = resources;
        }

        public object Post(HailRequest request)
        {
            _logger.LogMessage(string.Format("Starting Hail. Request : {0}", request.ToJson()));

            var createOrderRequest = Mapper.Map<CreateOrderRequest>(request);

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var createReportOrder = CreateReportOrder(createOrderRequest, account);

            var createOrderCommand = BuildCreateOrderCommand(createOrderRequest, account, createReportOrder);

            var result = _ibsCreateOrderService.CreateIbsOrder(createOrderCommand.AccountId, createOrderCommand.OrderId, createOrderCommand.PickupAddress,
                createOrderCommand.DropOffAddress, createOrderCommand.Settings.AccountNumber, createOrderCommand.Settings.CustomerNumber,
                createOrderCommand.CompanyKey, createOrderCommand.IbsAccountId, createOrderCommand.Settings.Name, createOrderCommand.Settings.Phone,
                createOrderCommand.Settings.Passengers, createOrderCommand.Settings.VehicleTypeId, createOrderCommand.IbsInformationNote, createOrderCommand.IsFutureBooking,
                createOrderCommand.PickupDate, createOrderCommand.Prompts, createOrderCommand.PromptsLength, createOrderCommand.ReferenceDataCompanyList,
                createOrderCommand.Market, createOrderCommand.Settings.ChargeTypeId, createOrderCommand.Settings.ProviderId, createOrderCommand.Fare,
                createOrderCommand.TipIncentive, account.DefaultTipPercent, true, createOrderCommand.CompanyFleetId);

            if (result.OrderKey.IbsOrderId > -1)
            {
                // TODO: cancel ibsorder if vehicle candidates list is empty

                createOrderCommand.IbsOrderId = result.OrderKey.IbsOrderId;

                _commandBus.Send(createOrderCommand);
            }

            return result;
        }

        public object Post(ConfirmHailRequest request)
        {
            if (request.OrderKey.IbsOrderId < 0)
            {
                throw new HttpError(string.Format("Cannot confirm hail because IBS returned error code {0}", request.OrderKey.IbsOrderId));
            }

            if (request.VehicleCandidate == null)
            {
                throw new HttpError("You need to specify a vehicle in order to confirm the hail.");
            }

            var orderDetail = _orderDao.FindById(request.OrderKey.TaxiHailOrderId);
            if (orderDetail == null)
            {
                throw new HttpError(string.Format("Order {0} doesn't exist", request.OrderKey.TaxiHailOrderId));
            }

            _logger.LogMessage(string.Format("Trying to confirm Hail. Request : {0}", request.ToJson()));

            try
            {
                var ibsOrderKey = Mapper.Map<IbsOrderKey>(request.OrderKey);
                var ibsVehicleCandidate = Mapper.Map<IbsVehicleCandidate>(request.VehicleCandidate);

                _dispatcherService.AssignJobToVehicle(orderDetail.CompanyKey, ibsOrderKey, ibsVehicleCandidate);

                return new OrderStatusDetail
                {
                    OrderId = request.OrderKey.TaxiHailOrderId,
                    Status = OrderStatus.Created,
                    IBSStatusId = VehicleStatuses.Common.Assigned,
                    IBSStatusDescription = string.Format(
                        _resources.Get("OrderStatus_CabDriverNumberAssigned", orderDetail.ClientLanguageCode),
                        request.VehicleCandidate.VehicleId)
                };
            }
            catch (Exception ex)
            {
                return new HttpResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
