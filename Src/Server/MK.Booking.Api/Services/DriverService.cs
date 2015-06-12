using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class DriverService : Service
    {
        private readonly IIbsOrderService _ibsOrderService;
        private readonly ILogger _logger;

        public DriverService(IIbsOrderService ibsOrderService, ILogger logger)
        {
            _ibsOrderService = ibsOrderService;
            _logger = logger;
        }

        public object Post(SendMessageToDriverRequest request)
        {
            try
            {
                _ibsOrderService.SendMessageToDriver(request.Message, request.VehicleNumber);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(
                    string.Format("An error occured when trying to send a message to the driver. Message: {0}, VehicleNumber: {1}",
                    request.Message, request.VehicleNumber));
                _logger.LogError(ex);

                throw new HttpError(HttpStatusCode.InternalServerError, ex.Message);
            }
            
            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
