using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Web.Security;
using AutoMapper;

namespace apcurium.MK.Web.Controllers.Api
{
    [NoCache]
    [Auth]
    public class OrderStatusController : BaseApiController
    {
        public OrderStatusService OrderStatusService { get; }

        public ActiveOrderStatusService ActiveOrderStatusService { get; }

        public OrderStatusController(OrderStatusHelper orderStatusHelper, IAccountDao accountDao, IOrderDao orderDao)
        {
            OrderStatusService = new OrderStatusService(orderStatusHelper);

            ActiveOrderStatusService = new ActiveOrderStatusService(orderDao, accountDao);
        }

        [HttpGet]
        [Route("api/v2/accounts/orders/{orderId}/status")]
        public async Task<IHttpActionResult> GetOrderStatus(Guid orderId)
        {
            var session = Session;

            var status = await OrderStatusService.Get(new OrderStatusRequest() {OrderId = orderId});

            return GenerateActionResult(status);
        }

        [HttpGet]
        [Route("api/v2/accounts/orders/status/active")]
        public IHttpActionResult GetActiveOrdersStatus()
        {
            var status = ActiveOrderStatusService.GetActiveOrders();

            return GenerateActionResult(status);
        }

        [HttpGet, Route("api/v2/accounts/orders/active")]
        public IHttpActionResult GetActiveAccount()
        {
            var status = ActiveOrderStatusService.GetActiveOrder();

            return GenerateActionResult(status);
        }
    }
}
