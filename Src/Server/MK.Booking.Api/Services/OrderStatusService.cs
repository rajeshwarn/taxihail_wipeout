using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<OrderStatusRequest>
    {
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public OrderStatusService(IBookingWebServiceClient bookingWebServiceClient)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        public override object OnGet(OrderStatusRequest request)
        {
            var status = new OrderStatus();
            try
            {
                var accountId = this.GetSession().UserAuthId; //TODO avoir le ibsaccountid => pas de requête
                var statusDetails = _bookingWebServiceClient.GetOrderStatus(request.OrderId, 0);
                status.Step = statusDetails.Item1;
                status.Latitude = statusDetails.Item2;
                status.Longititude = statusDetails.Item3;

            }catch
            {
                //TODO: erreur ? Status ?
            }
            

            return status;
        }
    }
}