using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using ServiceStack.Text;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class PromoCode
    {
        public PromoCode()
        {
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

        [Display(Name = "Start Time")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

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
    }
}