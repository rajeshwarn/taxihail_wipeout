using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OverduePaymentSettled : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public bool? IsPayInTaxiEnabled { get; set; }
    }
}