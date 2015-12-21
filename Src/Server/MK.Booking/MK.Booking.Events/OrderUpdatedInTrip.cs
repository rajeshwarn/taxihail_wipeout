using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class OrderUpdatedInTrip : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public Address DropOffAddress { get; set; }
    }
}