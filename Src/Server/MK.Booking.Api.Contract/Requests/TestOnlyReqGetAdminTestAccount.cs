#region


#endregion

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/account/test/admin/{Index}", "GET")]
    public class TestOnlyReqGetAdminTestAccount : BaseDto
    {
        public string Index { get; set; }
    }
}