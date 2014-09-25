using System;
using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeDeleted : VersionedEvent
    {
        public Guid RatingTypeId { get; set; }

        public IEnumerable<string> Languages { get; set; } 
    }
}