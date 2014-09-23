using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeDeleted : VersionedEvent
    {
        public Guid RatingTypeId { get; set; }
    }
}
