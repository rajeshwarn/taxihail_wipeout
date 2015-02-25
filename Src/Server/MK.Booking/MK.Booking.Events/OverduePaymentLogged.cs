using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OverduePaymentLogged : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
}