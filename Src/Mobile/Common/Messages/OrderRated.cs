using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderRated: GenericTinyMessage<string>
    {
        public OrderRated(object sender, string orderId)
            : base(sender, orderId)
        {            
        }
    }
}