using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserTaxiHailNetworkSettingsAddedOrUpdated : VersionedEvent
    {
        public bool IsEnabled { get; set; }

        public List<string> DisabledFleets { get; set; }
    }
}
