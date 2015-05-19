using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class TemporaryOrderPaymentInfoDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public string Cvv { get; set; }
    }
}