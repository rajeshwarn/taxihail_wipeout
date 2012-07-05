using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class OrderStatusClient: BaseServiceClient
    {
        public OrderStatusClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }

        public OrderStatus GetStatus(int orderId)
        {
            var result = Client.Get<OrderStatus>("/orderstatus/" + orderId.ToString(CultureInfo.InvariantCulture));
            return result;
        }
         
    }
}