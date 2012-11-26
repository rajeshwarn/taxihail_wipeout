using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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