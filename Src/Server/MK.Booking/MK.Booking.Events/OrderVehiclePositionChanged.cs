#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderVehiclePositionChanged : VersionedEvent
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}