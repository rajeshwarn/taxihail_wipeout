using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Post, Permissions.Admin)]
    [AuthorizationRequired(ApplyTo.Put, Permissions.Admin)]
    [RestService("/settings", "POST")]
    [RestService("/settings/{Key}", "PUT")]
    [RestService("/settings", "GET")]
    public class ConfigurationsRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public AppSettingsType AppSettingsType { get; set; }
    }
}
