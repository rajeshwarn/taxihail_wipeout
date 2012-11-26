using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderCanceled: GenericTinyMessage<Order>
    {

        public OrderCanceled(object sender, Order order, string ownerId)
            : base(sender, order)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}