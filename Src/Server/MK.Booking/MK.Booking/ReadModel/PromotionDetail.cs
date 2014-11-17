using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string DaysOfWeek { get; set; }

        public bool AppliesToCurrentBooking { get; set; }

        public bool AppliesToFutureBooking { get; set; }

        public double DiscountValue { get; set; }

        public PromoDiscountType DiscountType { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public int? MaxUsage { get; set; }

        public string Code { get; set; }

        public bool Active { get; set; }
    }
}