using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderController : BaseApiController
    {
        private readonly CancelOrderService _cancelOrderService;
        private readonly OrderService _orderService;


        public OrderController(IAccountDao accountDao, IOrderDao orderDao, IOrderPaymentDao orderPaymentDao, IPromotionDao promotionDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings, ITaxiHailNetworkServiceClient networkServiceClient, IIbsCreateOrderService ibsCreateOrderService, IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _cancelOrderService = new CancelOrderService(commandBus, ibsServiceProvider, orderDao, accountDao, updateOrderStatusJob, serverSettings, networkServiceClient, ibsCreateOrderService, Logger)
            {
                HttpRequestContext = RequestContext,
                Session = GetSession()
            };

            _orderService = new OrderService(orderDao, orderPaymentDao, promotionDao, accountDao, commandBus, ibsServiceProvider)
            {
                HttpRequestContext = RequestContext,
                Session = GetSession()
            };
        }

        [HttpGet]
        [Route("{orderId}")]
        public IHttpActionResult GetOrder(Guid orderId)
        {
            var result = _orderService.Get(new OrderRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete]
        [Route("{orderId}")]
        public IHttpActionResult DeleteOrder(Guid orderId)
        {
            _orderService.Delete(new OrderRequest() {OrderId = orderId});

            return Ok();
        }

        [HttpGet]
        [Route("{orderId}/calldriver")]
        public IHttpActionResult InitiateCallToDriver(Guid orderId)
        {
            var result = _orderService.Get(new InitiateCallToDriverRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpPost]
        [Route("{orderId}/cancel")]
        public IHttpActionResult CancelOrder(Guid orderId)
        {
            _cancelOrderService.Post(new Booking.Api.Contract.Requests.CancelOrder {OrderId = orderId});

            return Ok();
        }

    }
}
