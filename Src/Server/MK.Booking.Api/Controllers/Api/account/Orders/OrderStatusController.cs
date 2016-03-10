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
    [RoutePrefix("api/v2/accounts/orders")]
    [NoCache]
    [Auth]
    public class OrderStatusController : BaseApiController
    {
        private readonly OrderStatusService _orderStatusService;

        private readonly ActiveOrderStatusService _activeOrderStatusService;

        public OrderStatusController(OrderStatusHelper orderStatusHelper, IAccountDao accountDao, IOrderDao orderDao)
        {
            _orderStatusService = new OrderStatusService(orderStatusHelper);

            _activeOrderStatusService = new ActiveOrderStatusService(orderDao, accountDao);
        }
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_orderStatusService, _activeOrderStatusService);
        }

        [HttpGet]
        [Route("{OrderId}/status/")]
        public async Task<IHttpActionResult> GetOrderStatus(Guid orderId)
        {
            var status = await _orderStatusService.Get(new OrderStatusRequest() {OrderId = orderId});

            return GenerateActionResult(status);
        }

        [HttpGet]
        [Route("status/active")]
        public IHttpActionResult GetActiveOrdersStatus()
        {
            var status = _activeOrderStatusService.Get();

            return GenerateActionResult(status);
        }
    }
}
