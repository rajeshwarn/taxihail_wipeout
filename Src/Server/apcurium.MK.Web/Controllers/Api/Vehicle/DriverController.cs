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
        private readonly IIbsOrderService _ibsOrderService;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;

        public DriverController(IIbsOrderService ibsOrderService, ILogger logger, IOrderDao orderDao)
        {
            _ibsOrderService = ibsOrderService;
            _logger = logger;
            _orderDao = orderDao;
        }

        [HttpPost, Auth]
        [Route("/vehicle/{vehicleNumber}/message")]
        public HttpResponseMessage SendMessageToDriver(string vehicleNumber, SendMessageToDriverRequest request)
        {
            try
            {
                string companyKey = null;

                if (request.OrderId != Guid.Empty)
                {
                    var order = _orderDao.FindById(request.OrderId);
                    if (order == null)
                    {
                        throw GetException(HttpStatusCode.NotFound, "Order Id: {0} does not exist".InvariantCultureFormat(request.OrderId));
                    }

                    companyKey = order.CompanyKey;
                }

                _ibsOrderService.SendMessageToDriver(request.Message, request.VehicleNumber, companyKey);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(
                    string.Format("An error occured when trying to send a message to the driver. Message: {0}, VehicleNumber: {1}",
                    request.Message, request.VehicleNumber));
                _logger.LogError(ex);

                throw GetException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
