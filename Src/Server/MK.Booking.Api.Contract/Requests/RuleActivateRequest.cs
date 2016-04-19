#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/rules/{RuleId}/activate", "POST")]
    public class RuleActivateRequest
    {
        public Guid RuleId { get; set; }
    }
}