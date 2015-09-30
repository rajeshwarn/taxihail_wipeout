using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class DriverService : Service
    {
        private readonly IIbsOrderService _ibsOrderService;
        private readonly ILogger _logger;
	    private readonly IOrderDao _orderDao;

        public DriverService(IIbsOrderService ibsOrderService, ILogger logger, IOrderDao orderDao)
        {
            _ibsOrderService = ibsOrderService;
            _logger = logger;
	        _orderDao = orderDao;
        }

        public object Post(SendMessageToDriverRequest request)
        {
            try
            {
	            var order = _orderDao.FindById(request.OrderId);

                _ibsOrderService.SendMessageToDriver(request.Message, request.VehicleNumber, order.CompanyKey);
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
