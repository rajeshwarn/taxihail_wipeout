using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("api/v2/account")]
    public class AccountOrderController : BaseApiController
    {
        private readonly AccountOrderListService _service;

        public AccountOrderController(IOrderDao orderDao, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings)
        {
            _service = new AccountOrderListService(orderDao, orderRatingsDao, serverSettings);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_service);
        }

        [HttpGet]
        [Route("ordercountforapprating")]
        public IHttpActionResult GetOrderCountForAppRating()
        {
            return GenerateActionResult(_service.Get(new OrderCountForAppRatingRequest()));
        }

        [HttpGet, NoCache]
        [Route("orders")]
        public IHttpActionResult GetOrderListForAccount()
        {
            return GenerateActionResult(_service.Get(new AccountOrderListRequest()));
        }

    }
}
