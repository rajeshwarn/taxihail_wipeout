using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class VehicleIdMappingAdded : VersionedEvent
    {
        public string LegacyDispatchId { get; set; }

        public string DeviceName { get; set; }
    }
}
