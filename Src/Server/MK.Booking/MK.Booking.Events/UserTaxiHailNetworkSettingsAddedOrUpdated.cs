using apcurium.MK.Common.Configuration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserTaxiHailNetworkSettingsAddedOrUpdated : VersionedEvent
    {
        public UserTaxiHailNetworkSettings UserTaxiHailNetworkSettings { get; set; }
    }
}
