using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [Auth]
    public class OrderController : BaseApiController
    {
        public CancelOrderService CancelOrderService { get; private set; }
        public OrderService OrderService { get; private set; }
        public CreateOrderService CreateOrderService { get; private set; }
        public OrderUpdateService OrderUpdateService { get; private set; }
        public AccountOrderListService AccountOrderListService { get; private set; }
        public OrderStatusService OrderStatusService { get; private set; }
        public ActiveOrderStatusService ActiveOrderStatusService { get; private set; }
        public OrderPairingService PairingService { get; private set; }

        public OrderController(CancelOrderService cancelOrderService, OrderService orderService, CreateOrderService createOrderService, OrderUpdateService orderUpdateService, AccountOrderListService accountOrderListService, OrderStatusService orderStatusService, ActiveOrderStatusService activeOrderStatusService, OrderPairingService pairingService)
        {
            CancelOrderService = cancelOrderService;
            OrderService = orderService;
            CreateOrderService = createOrderService;
            OrderUpdateService = orderUpdateService;
            AccountOrderListService = accountOrderListService;
            OrderStatusService = orderStatusService;
            ActiveOrderStatusService = activeOrderStatusService;
            PairingService = pairingService;
        }

        [HttpPost, NoCache, Route("api/accounts/orders/{orderId}/pairing/tip")]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody]UpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            await PairingService.Post(request);

            return Ok();
        }

        [HttpGet, Route("api/accounts/orders/{orderId}/pairing"), NoCache]
        public IHttpActionResult GetOrderPairing(Guid orderId)
        {
            var result = PairingService.Get(new OrderPairingRequest() { OrderId = orderId });

            return GenerateActionResult(result);
        }

        [HttpGet, NoCache]
        [Route("api/accounts/orders/{orderId}/status")]
        public async Task<IHttpActionResult> GetOrderStatus(Guid orderId)
        {
            var session = Session;

            var status = await OrderStatusService.Get(new OrderStatusRequest() { OrderId = orderId });

            return GenerateActionResult(status);
        }

        [HttpGet, NoCache]
        [Route("api/accounts/orders/status/active")]
        public IHttpActionResult GetActiveOrdersStatus()
        {
            var status = ActiveOrderStatusService.GetActiveOrders();

            return GenerateActionResult(status);
        }

        [HttpGet, NoCache, Route("api/accounts/orders/active")]
        public IHttpActionResult GetActiveAccount()
        {
            var status = ActiveOrderStatusService.GetActiveOrder();

            return GenerateActionResult(status);
        }

        [HttpGet, Route("api/accounts/orders/{orderId}")]
        public IHttpActionResult GetOrder(Guid orderId)
        {
            var result = OrderService.Get(new OrderRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/accounts/orders/{orderId}")]
        public IHttpActionResult DeleteOrder(Guid orderId)
        {
            OrderService.Delete(new OrderRequest() {OrderId = orderId});

            return Ok();
        }

        [HttpGet, Route("api/accounts/orders/{orderId}/calldriver")]
        public IHttpActionResult InitiateCallToDriver(Guid orderId)
        {
            var result = OrderService.Get(new InitiateCallToDriverRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/accounts/orders/{orderId}/cancel")]
        public async Task<IHttpActionResult> CancelOrder(Guid orderId)
        {
            await CancelOrderService.Post(new CancelOrder {OrderId = orderId});

            return Ok();
        }

        [HttpPost, Route("api/accounts/orders/{orderId}/updateintrip")]
        public IHttpActionResult UpdateOrder(Guid orderId, [FromBody]OrderUpdateRequest request)
        {
            request.OrderId = orderId;

            var result = OrderUpdateService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/accounts/orders")]
        public async Task<IHttpActionResult> CreateOrder([FromBody]CreateOrderRequest request)
        {
            var result = await CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/accounts/orders/{orderId}/switchDispatchCompany")]
        public async Task<IHttpActionResult> SwitchOrderToNextDispatchCompany(Guid orderId, [FromBody] SwitchOrderToNextDispatchCompanyRequest request)
        {
            request.OrderId = orderId;

            var result = await CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/accounts/orders/{orderId}/ignoreDispatchCompanySwitch")]
        public IHttpActionResult IgnoreDispatchCompanySwitch(Guid orderId, [FromBody] IgnoreDispatchCompanySwitchRequest request)
        {
            request.OrderId = orderId;

            CreateOrderService.Post(request);

            return Ok();
        }

        [HttpGet, NoCache, Route("api/accounts/orders")]
        public IHttpActionResult GetOrderListForAccount()
        {
            var result = AccountOrderListService.Get(new AccountOrderListRequest());

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/accounts/orders/countforapprating")]
        public IHttpActionResult GetOrderCountForAppRating()
        {
            var result = AccountOrderListService.Get(new OrderCountForAppRatingRequest());

            return GenerateActionResult(result);
        }
    }
}
