#region

using ServiceStack.ServiceHost;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/ibschargeaccount/all", "GET")]
    public class IbsChargeAccountsRequest : IReturn<List<IbsChargeAccountResponse>>
    {
    }
}