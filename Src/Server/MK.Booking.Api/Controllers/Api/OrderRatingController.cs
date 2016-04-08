using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    [Auth]
    public class OrderRatingController : BaseApiController
    {
        public OrderRatingsService OrderRatingsService { get; private set; }

        public OrderRatingController(IOrderRatingsDao orderRatingDao, ICommandBus commandBus)
        {
            OrderRatingsService = new OrderRatingsService(orderRatingDao, commandBus);
        }

        [HttpGet, Route("api/v2/ratings/{orderId}")]
        public IHttpActionResult GetOrderRatings(Guid orderId)
        {
            var result = OrderRatingsService.Get(orderId);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/ratings")]
        public IHttpActionResult CommitOrderRating([FromBody] OrderRatingsRequest request)
        {
            OrderRatingsService.Post(request);

            return Ok();
        }
    }
}
