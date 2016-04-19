#region

using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/ibschargeaccount", "POST")]
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