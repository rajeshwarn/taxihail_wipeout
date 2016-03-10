using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api.Maps
{
    [RoutePrefix("api/v2/geocode")]
    public class GeocodingController : BaseApiController
    {
        public GeocodingService GeocodingService { get; }

        public GeocodingController(IGeocoding geocoding, IAccountDao accountDao)
        {
            GeocodingService = new GeocodingService(geocoding, accountDao);
        }

        [HttpPost]
        public IHttpActionResult GetAddressWithGeocoding([FromBody] GeocodingRequest request)
        {
            var result = GeocodingService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
