using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/encryptedsettings", "GET")]
	public class EncryptedConfigurationsRequest
    {
        public Dictionary<string, string> AppSettings { get; set; }

		public AppSettingsType AppSettingsType { get; set; }
    }
}