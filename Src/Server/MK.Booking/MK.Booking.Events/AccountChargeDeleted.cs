using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountChargeDeleted : VersionedEvent
    {
        public Guid AccountChargeId { get; set; }
    }
}