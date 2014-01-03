#region

using System;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/admin/rules/{RuleId}/deactivate", "POST")]
    public class RuleDeactivateRequest
    {
        public Guid RuleId { get; set; }
    }
}