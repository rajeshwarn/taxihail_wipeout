using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class FlightInformationController : BaseApiController
    {
        private readonly FlightInformationService _flightInformationService;

        public FlightInformationController(IServerSettings serverSettings)
        {
            _flightInformationService = new FlightInformationService(serverSettings)
            {
                HttpRequestContext = RequestContext,
                Session = GetSession()
            };
        }

        [HttpPost, Auth]
        [Route("flightInfo")]
        public IHttpActionResult GetFlightInformation(FlightInformationRequest request)
        {
            var result = _flightInformationService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
