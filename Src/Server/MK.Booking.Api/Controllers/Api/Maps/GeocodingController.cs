using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    public class GeocodingController : BaseApiController
    {
        public GeocodingService GeocodingService { get; private set; }

        public GeocodingController(GeocodingService geocodingService)
        {
            GeocodingService = geocodingService;
        }

        [HttpPost, Route("api/v2/geocode")]
        public IHttpActionResult GetAddressWithGeocoding([FromBody] GeocodingRequest request)
        {
            var result = GeocodingService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
