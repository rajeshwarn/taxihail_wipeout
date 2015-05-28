using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class FeesDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string Market { get; set; }

        public decimal Booking { get; set; }

        public decimal Cancellation { get; set; }

        public decimal NoShow { get; set; }
    }
}
