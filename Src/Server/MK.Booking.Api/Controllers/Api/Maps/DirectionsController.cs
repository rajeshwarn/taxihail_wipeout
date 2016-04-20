using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;

namespace apcurium.MK.Web.Controllers.Api
{
    public class DirectionsController : BaseApiController
    {
        public DirectionsService DirectionsService { get; private set; }

        public DirectionsController(DirectionsService directionsService)
        {
            DirectionsService = directionsService;
        }

        [HttpGet, Route("api/directions")]
        public async Task<IHttpActionResult> GetDirections([FromUri]DirectionsRequest request)
        {
            var result = await DirectionsService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/directions/eta")]
        public IHttpActionResult GetAssignedEta([FromUri] AssignedEtaRequest request)
        {
            var result = DirectionsService.Get(request);

            return GenerateActionResult(result);
        }
    }
}
