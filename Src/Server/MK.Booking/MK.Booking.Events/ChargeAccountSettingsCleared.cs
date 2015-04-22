using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class ChargeAccountSettingsCleared : VersionedEvent
    {
        public Guid AccountId { get; set; }
    }
}
