using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderCompleted : VersionedEvent
    {
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
    }
}