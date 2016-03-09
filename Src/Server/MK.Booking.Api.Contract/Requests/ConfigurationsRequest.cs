#region

using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/settings", "GET, POST")]
    public class ConfigurationsRequest
    {
        public Dictionary<string, string> AppSettings { get; set; }
        public AppSettingsType AppSettingsType { get; set; }
    }
}