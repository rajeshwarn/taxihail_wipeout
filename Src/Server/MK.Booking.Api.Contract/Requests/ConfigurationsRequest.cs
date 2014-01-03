#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate(ApplyTo.Post)]
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
    [Route("/settings", "GET, POST")]
    public class ConfigurationsRequest
    {
        public Dictionary<string, string> AppSettings { get; set; }
        public AppSettingsType AppSettingsType { get; set; }
    }
}