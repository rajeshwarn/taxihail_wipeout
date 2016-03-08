using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    [RoutePrefix("api/v2/places")]
    public class PlacesController : BaseApiController
    {
        public NearbyPlacesService NearbyPlacesService { get; }
        public PlaceDetailService PlaceDetailService { get; }

        public PlacesController(IPlaces client, IAccountDao accountDao)
        {
            NearbyPlacesService = new NearbyPlacesService(client, accountDao);
            PlaceDetailService = new PlaceDetailService(client);
        }

        [HttpGet]
        public IHttpActionResult GetNearbyPlace([FromUri] NearbyPlacesRequest request)
        {
            var result = NearbyPlacesService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("detail")]
        public async Task<IHttpActionResult> GetPlaceDetail([FromUri] PlaceDetailRequest request)
        {
            var result = await PlaceDetailService.Get(request);

            return GenerateActionResult(result);
        }

    }
}
