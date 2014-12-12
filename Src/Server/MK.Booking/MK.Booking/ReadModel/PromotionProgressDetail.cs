using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionProgressDetail
    {
        [Key, Column(Order = 1)]
        public Guid AccountId { get; set; }

        [Key, Column(Order = 2)]
        public Guid PromoId { get; set; }

        public int? RideCount { get; set; }

        public double? AmountSpent { get; set; }
    }
}
