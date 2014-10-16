#region

using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class CreditCardPaymentInitiated : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Meter { get; set; }
        public decimal Tip { get; set; }
        public PaymentProvider Provider { get; set; }
        public string CardToken { get; set; }
    }
}