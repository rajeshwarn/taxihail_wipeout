using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class FeesUpdated : VersionedEvent
    {
        public List<Fees> Fees { get; set; } 
    }
}