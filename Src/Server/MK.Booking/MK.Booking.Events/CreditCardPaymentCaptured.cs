using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardPaymentCaptured: VersionedEvent
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }
    }
}