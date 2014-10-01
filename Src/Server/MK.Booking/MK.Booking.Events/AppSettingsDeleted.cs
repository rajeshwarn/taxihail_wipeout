using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AppSettingsDeleted : VersionedEvent
    {
        public AppSettingsDeleted()
        {
            AppSettings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> AppSettings { get; set; }
    }
}
