using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeUpdated : VersionedEvent
    {
        public string Name { get; set; }
        public Guid RatingTypeId { get; set; } 
    }
}