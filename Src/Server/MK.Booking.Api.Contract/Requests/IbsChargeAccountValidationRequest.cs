#region

using System.Collections;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
    [Route("/ibschargeaccount/", "POST")]
    public class IbsChargeAccountValidationRequest : IReturn<IbsChargeAccountValidationResponse>
    {
        public IEnumerable<string> Prompts { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerNumber { get; set; }
    }

    public class IbsChargeAccountValidationResponse : IbsChargeAccountValidation
    {
    }
}