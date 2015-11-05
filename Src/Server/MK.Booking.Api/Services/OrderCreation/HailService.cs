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
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly Resources.Resources _resources;

        internal HailService(
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
            Resources.Resources resources)
            : base(serverSettings, commandBus, accountChargeDao, paymentService, creditCardDao,
                  ibsServiceProvider, promotionDao, promoRepository, orderPaymentDao, accountDao,
                  payPalServiceFactory, logger, taxiHailNetworkServiceClient, ruleCalculator,
                  feesDao, referenceDataService, orderDao)
        {
            _orderDao = orderDao;
            _logger = logger;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _accountDao = accountDao;
            _ibsCreateOrderService = ibsCreateOrderService;
            _resources = resources;
        }

        public object Post(HailRequest request)
        {
            _logger.LogMessage(string.Format("Starting Hail. Request : {0}", request.ToJson()));

            var createOrderRequest = Mapper.Map<CreateOrderRequest>(request);

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var createReportOrder = CreateReportOrder(createOrderRequest, account);

            var orderCommand = CreateOrder(createOrderRequest, account, createReportOrder);

            var result = _ibsCreateOrderService.CreateIbsOrder(request.Id, request.PickupAddress, request.DropOffAddress,
                request.Settings.AccountNumber, request.Settings.CustomerNumber, orderCommand.CompanyKey,
                orderCommand.IbsAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                request.Settings.VehicleTypeId, orderCommand.IbsInformationNote, request.PickupDate.Value, orderCommand.Prompts,
                orderCommand.PromptsLength, orderCommand.ReferenceDataCompanyList, orderCommand.Market, request.Settings.ChargeTypeId,
                request.Settings.ProviderId, orderCommand.Fare, createOrderRequest.TipIncentive, true);

            orderCommand.IbsOrderId = result.HailResult.OrderKey.IbsOrderId;

            _commandBus.Send(orderCommand);

            return result.HailResult;
        }

        public object Post(ConfirmHailRequest request)
        {
            var orderDetail = _orderDao.FindById(request.OrderKey.TaxiHailOrderId);
            if (orderDetail == null)
            {
                throw new HttpError(string.Format("Order {0} doesn't exist", request.OrderKey.TaxiHailOrderId));
            }

            _logger.LogMessage(string.Format("Trying to confirm Hail. Request : {0}", request.ToJson()));

            var ibsOrderKey = Mapper.Map<IbsOrderKey>(request.OrderKey);
            var ibsVehicleCandidate = Mapper.Map<IbsVehicleCandidate>(request.VehicleCandidate);

            var confirmHailResult = _ibsServiceProvider.Booking(orderDetail.CompanyKey).ConfirmHail(ibsOrderKey, ibsVehicleCandidate);
            if (confirmHailResult == null || confirmHailResult < 0)
            {
                var errorMessage = string.Format("Error while trying to confirm the hail. IBS response code : {0}", confirmHailResult);
                _logger.LogMessage(errorMessage);

                return new HttpResult(HttpStatusCode.InternalServerError, errorMessage);
            }

            _logger.LogMessage("Hail request confirmed");

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
    }
}
