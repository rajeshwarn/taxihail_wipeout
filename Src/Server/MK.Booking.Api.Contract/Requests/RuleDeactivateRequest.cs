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
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif

    [RestService("/admin/rules/{RuleId}/deactivate", "POST")]
    public class RuleDeactivateRequest
    {
        public Guid RuleId { get; set; }
    }
}
