using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderController : BaseApiController
    {
        public CancelOrderService CancelOrderService { get;  }
        public OrderService OrderService { get; }

        public OrderUpdateService OrderUpdateService { get; }
        
        public OrderController(IAccountDao accountDao, IOrderDao orderDao, IOrderPaymentDao orderPaymentDao, IPromotionDao promotionDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings, ITaxiHailNetworkServiceClient networkServiceClient, IIbsCreateOrderService ibsCreateOrderService, IUpdateOrderStatusJob updateOrderStatusJob)
        {
            CancelOrderService = new CancelOrderService(commandBus, ibsServiceProvider, orderDao, accountDao, updateOrderStatusJob, serverSettings, networkServiceClient, ibsCreateOrderService, Logger);
            OrderService = new OrderService(orderDao, orderPaymentDao, promotionDao, accountDao, commandBus, ibsServiceProvider);
            OrderUpdateService = new OrderUpdateService(orderDao, commandBus, ibsServiceProvider);
        }

        [HttpGet]
        [Route("{orderId}")]
        public IHttpActionResult GetOrder(Guid orderId)
        {
            var result = OrderService.Get(new OrderRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete]
        [Route("{orderId}")]
        public IHttpActionResult DeleteOrder(Guid orderId)
        {
            OrderService.Delete(new OrderRequest() {OrderId = orderId});

            return Ok();
        }

        [HttpGet]
        [Route("{orderId}/calldriver")]
        public IHttpActionResult InitiateCallToDriver(Guid orderId)
        {
            var result = OrderService.Get(new InitiateCallToDriverRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpPost]
        [Route("{orderId}/cancel")]
        public IHttpActionResult CancelOrder(Guid orderId)
        {
            CancelOrderService.Post(new CancelOrder {OrderId = orderId});

            return Ok();
        }

        [HttpPost, Route("{orderId}/updateintrip")]
        public IHttpActionResult UpdateOrder(Guid orderId, [FromBody]OrderUpdateRequest request)
        {
            request.OrderId = orderId;

            var result = OrderUpdateService.Post(request);

            return GenerateActionResult(result);
        }

    }
}
