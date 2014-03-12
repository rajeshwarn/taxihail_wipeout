using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CallboxOrderCreated : TinyMessageBase
    {
        public Order Order;
        public OrderStatusDetail OrderStatus;
        public CallboxOrderCreated(object sender, Order order, OrderStatusDetail orderStatus) : base(sender)
        {
            Order = order;
            OrderStatus = orderStatus;
        }
    }
}