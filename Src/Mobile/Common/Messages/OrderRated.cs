using System;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderRated: GenericTinyMessage<Guid>
    {
        public OrderRated(object sender, Guid orderId)
            : base(sender, orderId)
        {            
        }
    }
}