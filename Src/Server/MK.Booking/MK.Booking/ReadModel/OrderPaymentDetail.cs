using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderPaymentDetail
    {
        [Key]
        public Guid PaymentId { get; set; }

        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal Meter { get; set; }
        public decimal Tip { get; set; }


        public string CardToken { get; set; }
        public string PayPalToken { get; set; }

        public PaymentType Type { get; set; }
        public PaymentProvider Provider { get; set; }

        public string PayPalPayerId { get; set; }
        public string TransactionId { get; set; }
        public string AuthorizationCode { get; set; }

        public bool IsCancelled { get; set; }
        public bool IsCompleted { get; set; }
    }
}