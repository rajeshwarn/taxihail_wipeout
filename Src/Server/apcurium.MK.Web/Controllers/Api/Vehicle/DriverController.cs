using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
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
        private readonly DriverService _driverService;
        public DriverController(IIbsOrderService ibsOrderService, ILogger logger, IOrderDao orderDao)
        {
            _driverService = new DriverService(ibsOrderService, logger, orderDao);
        }

        [HttpPost, Auth]
        [Route("/vehicle/{vehicleNumber}/message")]
        public IHttpActionResult SendMessageToDriver(string vehicleNumber, SendMessageToDriverRequest request)
        {
            request.VehicleNumber = vehicleNumber;

            _driverService.Post(request);

            return Ok();
        }
    }
}
