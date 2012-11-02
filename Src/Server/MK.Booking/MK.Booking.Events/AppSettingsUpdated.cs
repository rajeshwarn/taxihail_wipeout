using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;

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
