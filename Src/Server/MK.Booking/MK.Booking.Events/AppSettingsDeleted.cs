using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AppSettingsDeleted : VersionedEvent
    {
        public AppSettingsDeleted()
        {
            AppSettings = new List<string>();
        }

        public IList<string> AppSettings { get; set; }
    }
}
