#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/rules/{RuleId}/deactivate", "POST")]
    public class RuleDeactivateRequest
    {
        public Guid RuleId { get; set; }
    }
}