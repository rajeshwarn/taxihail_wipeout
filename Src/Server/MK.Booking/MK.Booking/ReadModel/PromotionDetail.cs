using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}