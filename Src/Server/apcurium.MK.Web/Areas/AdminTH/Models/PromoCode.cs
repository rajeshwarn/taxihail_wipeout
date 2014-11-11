using System;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using ServiceStack.Text;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class PromoCode
    {
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

        public string Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DayOfWeek[] DaysOfWeek { get; set; }

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