#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/rules/{RuleId}/activate", "POST")]
    public class RuleActivateRequest
    {
        public Guid RuleId { get; set; }
    }
}