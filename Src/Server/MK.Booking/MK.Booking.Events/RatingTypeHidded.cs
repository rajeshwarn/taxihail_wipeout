using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeHidded : VersionedEvent
    {
        public Guid RatingTypeId { get; set; } 
    }
}