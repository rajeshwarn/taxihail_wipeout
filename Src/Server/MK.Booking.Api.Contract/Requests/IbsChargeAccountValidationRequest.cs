#region

using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/ibschargeaccount", "POST")]
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