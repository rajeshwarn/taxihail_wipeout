using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    public class PlacesController : BaseApiController
    {
        public NearbyPlacesService NearbyPlacesService { get; private set; }
        public PlaceDetailService PlaceDetailService { get; private set; }

        public PlacesController(NearbyPlacesService nearbyPlacesService, PlaceDetailService placeDetailService)
        {
            NearbyPlacesService = nearbyPlacesService;
            PlaceDetailService = placeDetailService;
        }

        [HttpGet, Route("api/v2/places")]
        public IHttpActionResult GetNearbyPlace([FromUri] NearbyPlacesRequest request)
        {
            var result = NearbyPlacesService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/places/detail")]
        public async Task<IHttpActionResult> GetPlaceDetail([FromUri] PlaceDetailRequest request)
        {
            var result = await PlaceDetailService.Get(request);

            return GenerateActionResult(result);
        }

    }
}
