using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post, Permissions.Admin)]
#endif

    [RestService("/admin/rules/{RuleId}/activate", "POST")]
    public class RuleActivateRequest
    {
        public Guid RuleId { get; set; }
    }
}
