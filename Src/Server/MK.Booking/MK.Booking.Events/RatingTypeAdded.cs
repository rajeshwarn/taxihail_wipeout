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
        public Guid CompanGuid { get; set; }
        public string Language { get; set; }
    }
}