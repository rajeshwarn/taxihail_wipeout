using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PrepaidOrderPaymentInfoUpdated : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal Meter { get; set; }

        public decimal Tax { get; set; }

        public decimal Tip { get; set; }

        public string TransactionId { get; set; }

        public PaymentProvider Provider { get; set; }

        public PaymentType Type { get; set; }
    }
}
