using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderStatusChangedForManualRideLinq : VersionedEvent
    {
        public OrderStatus Status { get; set; }

        public DateTime LastTripPollingDateInUtc { get; set; }
    }
}