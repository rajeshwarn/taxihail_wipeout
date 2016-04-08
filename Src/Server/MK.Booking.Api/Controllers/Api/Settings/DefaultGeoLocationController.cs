using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    public class DefaultGeoLocationController : BaseApiController
    {
        public DefaultGeoLocationService DefaultGeoLocationService { get; private set; }

        public DefaultGeoLocationController(IServerSettings serverSettings)
        {
            DefaultGeoLocationService = new DefaultGeoLocationService(serverSettings);
        }

        [HttpGet, Route("api/v2/settings/defaultlocation")]
        public IHttpActionResult GetDefaultGeoLocation()
        {
            var result = DefaultGeoLocationService.Get();

            return GenerateActionResult(result);
        }
    }
}
