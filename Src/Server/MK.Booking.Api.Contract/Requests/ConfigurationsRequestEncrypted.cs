using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/settingsencrypted", "GET")]
    public class ConfigurationRequestEncrypted
    {
        public Dictionary<string, string> AppSettings { get; set; }

		public AppSettingsType AppSettingsType { get; set; }
    }
}