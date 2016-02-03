using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OverduePaymentLogged : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public Guid? CreditCardId { get; set; }

        public decimal Amount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public FeeTypes FeeType { get; set; }
    }
}