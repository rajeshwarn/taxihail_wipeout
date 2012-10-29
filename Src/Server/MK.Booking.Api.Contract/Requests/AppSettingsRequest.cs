using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, Permissions.Admin)]
    [RestService("/admin/appsettings", "GET")]
    [RestService("/admin/appsettings", "POST")]
    [RestService("/admin/appsettings/{Key}", "PUT")]
    public class AppSettingsRequest : BaseDTO
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}