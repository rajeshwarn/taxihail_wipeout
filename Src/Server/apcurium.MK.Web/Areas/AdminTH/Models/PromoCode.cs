using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class PromoCode
    {
        public PromoCode()
        {
            DaysOfWeek = new [] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            AppliesToCurrentBooking = true;
            AppliesToFutureBooking = true;
            DiscountType = PromoDiscountType.Cash;
        }

        public PromoCode(PromotionDetail promoDetail)
        {
            Id = promoDetail.Id;
            Name = promoDetail.Name;
            StartDate = promoDetail.StartDate;
            EndDate = promoDetail.EndDate;
            StartTime = promoDetail.StartTime;
            EndTime = promoDetail.EndTime;
            DaysOfWeek = promoDetail.DaysOfWeek.FromJson<DayOfWeek[]>();
            AppliesToCurrentBooking = promoDetail.AppliesToCurrentBooking;
            AppliesToFutureBooking = promoDetail.AppliesToFutureBooking;
            DiscountValue = promoDetail.DiscountValue;
            DiscountType = promoDetail.DiscountType;
            MaxUsagePerUser = promoDetail.MaxUsagePerUser;
            MaxUsage = promoDetail.MaxUsage;
            Code = promoDetail.Code;
            Active = promoDetail.Active;
        }

        public Guid Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        [Display(Name = "Start Time")]
        [RegularExpression(@"^$|^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid start time")]
        public string StartTimeValue
        {
            get { return StartTime.HasValue ? StartTime.Value.ToString("hh:mm tt") : string.Empty; }
            set { StartTime = SetTime(value); }
        }

        public DateTime? EndTime { get; set; }

        [Display(Name = "End Time")]
        [RegularExpression(@"^$|^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid end time")]
        public string EndTimeValue
        {
            get { return EndTime.HasValue ? EndTime.Value.ToString("hh:mm tt") : string.Empty; } 
            set { EndTime = SetTime(value); }
        }

        public DayOfWeek[] DaysOfWeek { get; set; }

        [Display(Name = "Current booking")]
        public bool AppliesToCurrentBooking { get; set; }

        [Display(Name = "Future booking")]
        public bool AppliesToFutureBooking { get; set; }

        [Display(Name = "Promo Discount")]
        [Required]
        public double DiscountValue { get; set; }

        [Required]
        public PromoDiscountType DiscountType { get; set; }

        [Display(Name = "Max Usage/User")]
        public int? MaxUsagePerUser { get; set; }

        [Display(Name = "Max Usage/System")]
        public int? MaxUsage { get; set; }

        [Display(Name = "Promo Code (5-10 characters)")]
        [Required]
        [StringLength(10, MinimumLength = 5)]
        public string Code { get; set; }

        public bool Active { get; set; }

        private DateTime? SetTime(string timeStringValue)
        {
            if (!timeStringValue.HasValue())
            {
                return null;
            }

            return DateTime.Parse(timeStringValue);
        }
    }
}