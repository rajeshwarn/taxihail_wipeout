using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionUsageDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public Guid PromoId { get; set; }

        public Guid AccountId { get; set; }

        public decimal DiscountValue { get; set; }

        public PromoDiscountType DiscountType { get; set; }

        public decimal AmountSaved { get; set; }

        public string Code { get; set; }
    }
}