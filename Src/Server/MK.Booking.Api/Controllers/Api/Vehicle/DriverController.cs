using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Vehicle
{
    public class DriverController : BaseApiController
    {
        public DriverService DriverService { get; private set; }

        public DriverController(DriverService driverService)
        {
            DriverService = driverService;
        }

        [HttpPost, Auth]
        [Route("api/v2/vehicle/{vehicleNumber}/message")]
        public IHttpActionResult SendMessageToDriver(string vehicleNumber, SendMessageToDriverRequest request)
        {
            request.VehicleNumber = vehicleNumber;

            DriverService.Post(request);

            return Ok();
        }
    }
}
