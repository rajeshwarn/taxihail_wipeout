#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class RatingTypeAdded : VersionedEvent
    {
        public string Name { get; set; }
        public Guid RatingTypeId { get; set; }
    }
}