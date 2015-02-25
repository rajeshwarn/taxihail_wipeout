using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class OverduePaymentDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public decimal OverdueAmount { get; set; }

        public bool IsPaid { get; set; }
    }
}
