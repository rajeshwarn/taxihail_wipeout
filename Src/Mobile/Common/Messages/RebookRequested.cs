using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class RebookRequested : GenericTinyMessage<Order>
    {
        public RebookRequested(object sender, Order order)
            : base(sender, order)
        {            
        }
    }
}