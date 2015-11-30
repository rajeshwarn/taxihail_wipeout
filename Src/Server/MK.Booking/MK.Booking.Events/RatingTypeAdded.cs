using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeAdded : VersionedEvent
    {
        public string Name { get; set; }
        public Guid RatingTypeId { get; set; }
        public string Language { get; set; }
    }
}