using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2")]
    public class PromotionController : BaseApiController
    {
        public PromotionService PromotionService { get; }

        public PromotionController(IPromotionDao promotionDao)
        {
            PromotionService = new PromotionService(promotionDao);
        }

        [HttpGet, Route("promotions")]
        public IHttpActionResult GetPromotions()
        {
            var result = PromotionService.Get();

            return GenerateActionResult(result);
        }
    }
}
