using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<OrderStatusRequest>
    {
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public OrderStatusService(IOrderDao dao)
        {
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public override object OnGet(OrderStatusRequest request)
        {
          
            /*try
            {

                var accountId = this.GetSession().UserAuthId; //TODO avoir le ibsaccountid => pas de requête
                //var statusDetails = _bookingWebServiceClient.GetOrderStatus(request.OrderId, 0);
                //status.Step = statusDetails.Item1;
                status.Latitude = statusDetails.Item2;
                status.Longitude = statusDetails.Item3;

            }catch
            {
                //TO DO: erreur ? Status ?
            }*/
            return Dao.FindById(request.OrderId).Status;
        }
    }
}