using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class PayPalExpressCheckoutPaymentDetail
    {
        [Key]
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
        public string PayPalPayerId { get; set; }
        public string TransactionId { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsCompleted { get; set; }

    }
}
