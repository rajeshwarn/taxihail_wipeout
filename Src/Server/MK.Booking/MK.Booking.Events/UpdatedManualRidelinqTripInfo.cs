using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UpdatedManualRidelinqTripInfo : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public double? Distance { get; set; }
        public double? Faire { get; set; }
        public double? Tax { get; set; }
        public double? Tip { get; set; }
        public double? Toll { get; set; }
        public double? Extra { get; set; }
        public DriverInfos DriverInfo { get; set; }
    }
}
