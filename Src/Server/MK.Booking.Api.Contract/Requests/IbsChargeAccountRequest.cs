#region

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/ibschargeaccount/{AccountNumber}/{CustomerNumber}", "GET")]
    public class IbsChargeAccountRequest : IReturn<IbsChargeAccountResponse>
    {
        public string AccountNumber { get; set; }
        public string CustomerNumber { get; set; }
    }

    public class IbsChargeAccountResponse : IbsChargeAccount
    {
        
    }
}