#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/rules/{RuleId}/deactivate", "POST")]
    public class RuleDeactivateRequest
    {
        public Guid RuleId { get; set; }
    }
}