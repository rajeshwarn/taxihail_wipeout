using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PromotionController : BaseApiController
    {
        public PromotionService PromotionService { get; private set; }

        public PromotionController(IPromotionDao promotionDao)
        {
            PromotionService = new PromotionService(promotionDao);
        }

        [HttpGet, Route("api/v2/promotions")]
        public IHttpActionResult GetPromotions()
        {
            var result = PromotionService.Get();

            return GenerateActionResult(result);
        }
    }
}
