using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    [RoutePrefix("api/v2/settings/defaultlocation")]
    public class DefaultGeoLocationController : BaseApiController
    {
        public DefaultGeoLocationService DefaultGeoLocationService { get; }

        public DefaultGeoLocationController(IServerSettings serverSettings)
        {
            DefaultGeoLocationService = new DefaultGeoLocationService(serverSettings);
        }

        public IHttpActionResult GetDefaultGeoLocation()
        {
            var result = DefaultGeoLocationService.Get();

            return GenerateActionResult(result);
        }
    }
}
