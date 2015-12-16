using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class PromoCode : IValidatableObject
    {
        public PromoCode()
        {
            // initial values
            DaysOfWeek = new [] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            AppliesToCurrentBooking = true;
            AppliesToFutureBooking = true;
            DiscountType = PromoDiscountType.Cash;
            TriggerSettings = new PromotionTriggerSettings();
            CanModifyTriggerGoal = true;
        }

        public PromoCode(PromotionDetail promoDetail) : this()
        {
            Id = promoDetail.Id;
            Name = promoDetail.Name;
            Description = promoDetail.Description;
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
            PublishedStartDate = promoDetail.PublishedStartDate;
            PublishedEndDate = promoDetail.PublishedEndDate;
            TriggerSettings = promoDetail.TriggerSettings ?? new PromotionTriggerSettings();
        }

        public Guid Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        [Display(Name = "Start Time")]
        [RegularExpression(@"^$|^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid start time")]
        public string StartTimeValue
        {
            get { return StartTime.HasValue ? StartTime.Value.ToString("hh:mm tt") : string.Empty; }
            set { StartTime = SetTime(value, false); }
        }

        public DateTime? EndTime { get; set; }

        [Display(Name = "End Time")]
        [RegularExpression(@"^$|^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid end time")]
        public string EndTimeValue
        {
            get { return EndTime.HasValue ? EndTime.Value.ToString("hh:mm tt") : string.Empty; } 
            set { EndTime = SetTime(value, true); }
        }

        public DayOfWeek[] DaysOfWeek { get; set; }

        [Display(Name = "Current booking")]
        public bool AppliesToCurrentBooking { get; set; }

        [Display(Name = "Future booking")]
        public bool AppliesToFutureBooking { get; set; }

        [Display(Name = "Promo Discount")]
        [Required]
        [Range(double.Epsilon, double.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        [Required]
        public PromoDiscountType DiscountType { get; set; }

        [Display(Name = "Max Usage/User")]
        public int? MaxUsagePerUser { get; set; }

        [Display(Name = "Max Usage/System")]
        public int? MaxUsage { get; set; }

        [Display(Name = "Promo Code (5-10 characters)")]
        [Required, StringLength(10, MinimumLength = 5)]
        public string Code { get; set; }

        [Display(Name = "Published Start Date")]
        public DateTime? PublishedStartDate { get; set; }

        [Display(Name = "Published End Date")]
        public DateTime? PublishedEndDate { get; set; }

        public bool Active { get; set; }

        public PromotionTriggerSettings TriggerSettings { get; set; }

        public bool CanModifyTriggerGoal { get; set; }

        public bool IsExpired
        {
            get
            {
                var isExpired = false;

                if (EndDate.HasValue && EndTime.HasValue)
                {
                    isExpired = EndDate.Value.Add(EndTime.Value.TimeOfDay) < DateTime.Now;
                }
                else if (EndDate.HasValue)
                {
                    isExpired = EndDate.Value.AddDays(1) < DateTime.Now;
                }
                return isExpired;
            }
        }

        private DateTime? SetTime(string timeStringValue, bool isEndTime)
        {
            if (!timeStringValue.HasValue())
            {
                return null;
            }

            var result = DateTime.Parse(timeStringValue);

            if (isEndTime)
            {
                if (StartTime >= result)
                {
                    result = result.AddDays(1);
                }
            }

            return result;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (StartDate.HasValue && EndDate.HasValue && StartDate >= EndDate)
            {
                results.Add(new ValidationResult("Start Date must be before End Date"));
            }

            if (PublishedStartDate.HasValue && PublishedEndDate.HasValue && PublishedStartDate >= PublishedEndDate)
            {
                results.Add(new ValidationResult("Published Start Date must be before Published End Date"));
            }

            if ((StartTime.HasValue && !EndTime.HasValue) || (!StartTime.HasValue && EndTime.HasValue))
            {
                results.Add(new ValidationResult("When defining a time range, you must specify both time values"));
            }

            if (StartTime.HasValue && EndTime.HasValue && StartTime >= EndTime)
            {
                results.Add(new ValidationResult("Start Time must be before End Time"));
            }

            if (!AppliesToCurrentBooking && !AppliesToFutureBooking)
            {
                results.Add(new ValidationResult("A promotion must apply to at least one type of booking"));
            }

            return results;
        }
    }
}