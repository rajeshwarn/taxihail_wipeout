using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/flightInfo")]
    public class FlightInformationController : BaseApiController
    {
        public FlightInformationService InformationService { get; private set;  }

        public FlightInformationController(FlightInformationService informationService)
        {
            InformationService = informationService;
        }

        [HttpPost, Auth]
        public IHttpActionResult GetFlightInformation(FlightInformationRequest request)
        {
            var result = InformationService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
