#region

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate(ApplyTo.All)]
    [AuthorizationRequired(ApplyTo.All, RoleName.Admin)]
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