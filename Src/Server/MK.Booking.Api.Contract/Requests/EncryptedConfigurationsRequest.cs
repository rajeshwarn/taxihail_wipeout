using apcurium.MK.Common.Enumeration;
using System.Collections.Generic;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/encryptedsettings", "GET")]
	public class EncryptedConfigurationsRequest
    {
        public Dictionary<string, string> AppSettings { get; set; }

		public AppSettingsType AppSettingsType { get; set; }
    }
}