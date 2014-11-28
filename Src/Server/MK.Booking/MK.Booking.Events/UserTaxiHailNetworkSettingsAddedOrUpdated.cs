using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserTaxiHailNetworkSettingsAddedOrUpdated : VersionedEvent
    {
        public bool IsEnabled { get; set; }

        public string[] DisabledFleets { get; set; }
    }
}
