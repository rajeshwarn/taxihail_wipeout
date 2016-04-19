#region

using System.Collections.Generic;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/ibschargeaccount/all", "GET")]
    public class IbsChargeAccountsRequest : IReturn<List<IbsChargeAccountResponse>>
    {
    }
}