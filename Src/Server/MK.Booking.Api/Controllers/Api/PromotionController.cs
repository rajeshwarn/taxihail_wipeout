using System.Web.Http;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PromotionController : BaseApiController
    {
        public PromotionService PromotionService { get; private set; }

        public PromotionController(PromotionService promotionService)
        {
            PromotionService = promotionService;
        }

        [HttpGet, Route("api/promotions")]
        public IHttpActionResult GetPromotions()
        {
            var result = PromotionService.Get();

            return GenerateActionResult(result);
        }
    }
}
