using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RateDeleted: VersionedEvent
    {
        public Guid RateId { get; set; }
    }
}
