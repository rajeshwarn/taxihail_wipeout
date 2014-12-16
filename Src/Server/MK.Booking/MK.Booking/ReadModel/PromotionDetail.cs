using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string DaysOfWeek { get; set; }

        public bool AppliesToCurrentBooking { get; set; }

        public bool AppliesToFutureBooking { get; set; }

        public decimal DiscountValue { get; set; }

        public PromoDiscountType DiscountType { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public int? MaxUsage { get; set; }

        public string Code { get; set; }

        public DateTime? PublishedStartDate { get; set; }

        public DateTime? PublishedEndDate { get; set; }

        public bool Active { get; set; }

        public PromotionTriggerSettings TriggerSettings { get; set; }

        public DateTime? GetStartDateTime()
        {
            if (!StartDate.HasValue)
            {
                return null;
            }

            return !StartTime.HasValue
                ? new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day)
                : new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day, StartTime.Value.Hour, StartTime.Value.Minute, 0);
        }

        public DateTime? GetEndDateTime()
        {
            if (!EndDate.HasValue)
            {
                return null;
            }

            return !EndTime.HasValue
                ? new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day)
                : new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, EndTime.Value.Hour, EndTime.Value.Minute, 0);
        }

        public string GetNoteToDriverFormattedString()
        {
            var discountType = DiscountType == PromoDiscountType.Cash
                ? "$"
                : "%";

            return string.Format("PROMO,{0},{1},{2}",
                Code,
                discountType,
                DiscountValue);
        }
    }
}