using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class DispatchCompanySwitchIgnored : VersionedEvent
    {
        public Guid OrderId { get; set; }
    }
}
