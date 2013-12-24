#region

using System.Collections.Generic;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AppSettingsAddedOrUpdated : VersionedEvent
    {
        public AppSettingsAddedOrUpdated()
        {
            AppSettings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> AppSettings { get; set; }
    }
}