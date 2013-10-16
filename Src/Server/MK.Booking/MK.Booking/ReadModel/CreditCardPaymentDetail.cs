using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class CreditCardPaymentDetail
    {
        [Key]
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public bool IsCaptured { get; set; }

        public string CardToken { get; set; }
    }
}