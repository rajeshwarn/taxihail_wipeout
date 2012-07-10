using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Events
{
    public class OrderCancelled : VersionedEvent
    {
        public OrderCancelled()
        {
            Status = OrderStatus.Cancelled.ToString();
        }
        public string Status { get; set; }
    }
}
