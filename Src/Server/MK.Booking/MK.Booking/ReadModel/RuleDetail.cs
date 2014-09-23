#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class RuleDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public int Category { get; set; }
        public string ZoneList { get; set; }
        public bool ZoneRequired { get; set; }
        public bool AppliesToCurrentBooking { get; set; }
        public bool AppliesToFutureBooking { get; set; }
        public bool AppliesToPickup { get; set; }
        public bool AppliesToDropoff { get; set; }
        public int DaysOfTheWeek { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }
}