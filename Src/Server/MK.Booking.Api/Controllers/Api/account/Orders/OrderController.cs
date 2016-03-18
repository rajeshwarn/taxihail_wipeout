using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
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
            IOrderRatingsDao orderRatingsDao)
        {
            AccountOrderListService = new AccountOrderListService(orderDao, orderRatingsDao, serverSettings);
            CancelOrderService = new CancelOrderService(commandBus, ibsServiceProvider, orderDao, accountDao, updateOrderStatusJob, serverSettings, networkServiceClient, ibsCreateOrderService, Logger);
            OrderService = new OrderService(orderDao, orderPaymentDao, promotionDao, accountDao, commandBus, ibsServiceProvider);
            OrderUpdateService = new OrderUpdateService(orderDao, commandBus, ibsServiceProvider);
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
