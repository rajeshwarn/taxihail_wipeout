using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Vehicle
{
    public class DriverController : BaseApiController
    {
        public DriverService DriverService { get; private set; }

        public DriverController(IIbsOrderService ibsOrderService, ILogger logger, IOrderDao orderDao)
        {
            DriverService = new DriverService(ibsOrderService, logger, orderDao);
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
