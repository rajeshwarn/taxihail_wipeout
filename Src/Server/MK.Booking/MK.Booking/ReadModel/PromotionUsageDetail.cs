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

        public string UserId { get; set; }

        public DateTime? DateRedeemed { get; set; }

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