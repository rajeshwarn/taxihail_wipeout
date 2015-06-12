using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.EventHandlers.Integration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class DriverService : Service
    {
        private readonly IIbsOrderService _ibsOrderService;

        public DriverService(IIbsOrderService ibsOrderService)
        {
            _ibsOrderService = ibsOrderService;
        }

        public object Post(SendMessageToDriverRequest request)
        {
            try
            {
                _ibsOrderService.SendMessageToDriver(request.Message, request.VehicleNumber);
            }
            catch (Exception ex)
            {
                throw new HttpError(HttpStatusCode.InternalServerError, ex.Message);
            }
            
            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
