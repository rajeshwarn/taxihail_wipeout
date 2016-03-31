using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [Auth]
    public class OrderController : BaseApiController
    {
        public CancelOrderService CancelOrderService { get;  }
        public OrderService OrderService { get; }
        public CreateOrderService CreateOrderService { get; }
        public OrderUpdateService OrderUpdateService { get; }
        private AccountOrderListService AccountOrderListService { get; }
        public OrderStatusService OrderStatusService { get; }
        public ActiveOrderStatusService ActiveOrderStatusService { get; }

        public OrderPairingService PairingService { get; }

        public OrderController(
            IAccountDao accountDao, 
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPromotionDao promotionDao, 
            ICommandBus commandBus, 
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings,
            ITaxiHailNetworkServiceClient networkServiceClient, 
            IIbsCreateOrderService ibsCreateOrderService,
            IUpdateOrderStatusJob updateOrderStatusJob,
            ICacheClient cacheClient,
            IRuleCalculator ruleCalculator,
            IAccountChargeDao accountChargeDao,
            ICreditCardDao creditCardDao,
            IEventSourcedRepository<Promotion> promoRepository,
            IPaymentService paymentService,
            IPayPalServiceFactory payPalServiceFactory, 
            IFeesDao feesDao,
            IAirlineDao airlineDao,
            IPickupPointDao pickupPointDao,
            IOrderRatingsDao orderRatingsDao,
            OrderStatusHelper orderStatusHelper)
        {
            AccountOrderListService = new AccountOrderListService(orderDao, orderRatingsDao, serverSettings);
            CancelOrderService = new CancelOrderService(commandBus, ibsServiceProvider, orderDao, accountDao, updateOrderStatusJob, serverSettings, networkServiceClient, ibsCreateOrderService, Logger);
            OrderService = new OrderService(orderDao, orderPaymentDao, promotionDao, accountDao, commandBus, ibsServiceProvider);
            OrderUpdateService = new OrderUpdateService(orderDao, commandBus, ibsServiceProvider);
            PairingService = new OrderPairingService(orderDao, commandBus, serverSettings, paymentService);
            OrderStatusService = new OrderStatusService(orderStatusHelper);

            ActiveOrderStatusService = new ActiveOrderStatusService(orderDao, accountDao);

            CreateOrderService = new CreateOrderService(
                commandBus,
                accountDao,
                serverSettings,
                new ReferenceDataService(ibsServiceProvider, cacheClient, serverSettings, airlineDao, pickupPointDao), 
                ibsServiceProvider,
                ruleCalculator,
                accountChargeDao,
                creditCardDao,
                orderDao,
                promotionDao,
                promoRepository,
                networkServiceClient,
                paymentService,
                payPalServiceFactory,
                orderPaymentDao,
                feesDao,
                Logger,
                ibsCreateOrderService);
        }

        [HttpPost, NoCache, Route("api/v2/accounts/orders/{orderId}/pairing/tip")]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody]UpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            await PairingService.Post(request);

            return Ok();
        }

        [HttpGet, Route("api/v2/accounts/orders/{orderId}/pairing"), NoCache]
        public IHttpActionResult GetOrderPairing(Guid orderId)
        {
            var result = PairingService.Get(new OrderPairingRequest() { OrderId = orderId });

            return GenerateActionResult(result);
        }

        [HttpGet, NoCache]
        [Route("api/v2/accounts/orders/{orderId}/status")]
        public async Task<IHttpActionResult> GetOrderStatus(Guid orderId)
        {
            var session = Session;

            var status = await OrderStatusService.Get(new OrderStatusRequest() { OrderId = orderId });

            return GenerateActionResult(status);
        }

        [HttpGet, NoCache]
        [Route("api/v2/accounts/orders/status/active")]
        public IHttpActionResult GetActiveOrdersStatus()
        {
            var status = ActiveOrderStatusService.GetActiveOrders();

            return GenerateActionResult(status);
        }

        [HttpGet, NoCache, Route("api/v2/accounts/orders/active")]
        public IHttpActionResult GetActiveAccount()
        {
            var status = ActiveOrderStatusService.GetActiveOrder();

            return GenerateActionResult(status);
        }

        [HttpGet, Route("api/v2/accounts/orders/{orderId}")]
        public IHttpActionResult GetOrder(Guid orderId)
        {
            var result = OrderService.Get(new OrderRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/v2/accounts/orders/{orderId}")]
        public IHttpActionResult DeleteOrder(Guid orderId)
        {
            OrderService.Delete(new OrderRequest() {OrderId = orderId});

            return Ok();
        }

        [HttpGet, Route("api/v2/accounts/orders/{orderId}/calldriver")]
        public IHttpActionResult InitiateCallToDriver(Guid orderId)
        {
            var result = OrderService.Get(new InitiateCallToDriverRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/orders/{orderId}/cancel")]
        public async Task<IHttpActionResult> CancelOrder(Guid orderId)
        {
            await CancelOrderService.Post(new CancelOrder {OrderId = orderId});

            return Ok();
        }

        [HttpPost, Route("api/v2/accounts/orders/{orderId}/updateintrip")]
        public IHttpActionResult UpdateOrder(Guid orderId, [FromBody]OrderUpdateRequest request)
        {
            request.OrderId = orderId;

            var result = OrderUpdateService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/orders")]
        public async Task<IHttpActionResult> CreateOrder([FromBody]CreateOrderRequest request)
        {
            var result = await CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/orders/{orderId}/switchDispatchCompany")]
        public async Task<IHttpActionResult> SwitchOrderToNextDispatchCompany(Guid orderId, [FromBody] SwitchOrderToNextDispatchCompanyRequest request)
        {
            request.OrderId = orderId;

            var result = await CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/orders/{orderId}/ignoreDispatchCompanySwitch")]
        public IHttpActionResult IgnoreDispatchCompanySwitch(Guid orderId, [FromBody] IgnoreDispatchCompanySwitchRequest request)
        {
            request.OrderId = orderId;

            CreateOrderService.Post(request);

            return Ok();
        }

        [HttpGet, NoCache, Route("api/v2/accounts/orders")]
        public IHttpActionResult GetOrderListForAccount()
        {
            var result = AccountOrderListService.Get(new AccountOrderListRequest());

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/accounts/orders/countforapprating")]
        public IHttpActionResult GetOrderCountForAppRating()
        {
            var result = AccountOrderListService.Get(new OrderCountForAppRatingRequest());

            return GenerateActionResult(result);
        }
    }
}
