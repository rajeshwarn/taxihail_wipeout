#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class RuleUpdated : VersionedEvent
    {
        public Guid RuleId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string ZoneList { get; set; }
        public bool ZoneRequired { get; set; }
        public bool AppliesToCurrentBooking { get; set; }
        public bool AppliesToFutureBooking { get; set; }
        public bool AppliesToPickup { get; set; }
        public bool AppliesToDropoff { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }
}