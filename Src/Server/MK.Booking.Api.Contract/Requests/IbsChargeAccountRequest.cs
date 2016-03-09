#region

using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/ibschargeaccount/{AccountNumber}/{CustomerNumber}", "GET")]
    public class IbsChargeAccountRequest : IReturn<IbsChargeAccountResponse>
    {
        public string AccountNumber { get; set; }
        public string CustomerNumber { get; set; }
    }

    public class IbsChargeAccountResponse : IbsChargeAccount
    {
        
    }
}