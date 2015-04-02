using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class ManualRideLinqUnpaired : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public string RideLinqId { get; set; }
    }
}
