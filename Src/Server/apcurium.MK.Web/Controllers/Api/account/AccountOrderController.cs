using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class AccountOrderController : BaseApiController
    {
        private readonly AccountOrderListService _service;

        public AccountOrderController(IOrderDao orderDao, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings)
        {
            _service = new AccountOrderListService(orderDao, orderRatingsDao, serverSettings)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };
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
