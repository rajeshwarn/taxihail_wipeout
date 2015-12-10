using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class BraintreeAccountIdAdded : VersionedEvent
    {
        public Guid AccountId { get; set; }
        public string BraintreeAccountId { get; set; }
    }
}
