using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate(ApplyTo.Post)]
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
    [RestService("/settings", "GET, POST")]
    public class ConfigurationsRequest
    {
        public Dictionary<string, string> AppSettings { get; set; } 
        public AppSettingsType AppSettingsType { get; set; }
    }
}
