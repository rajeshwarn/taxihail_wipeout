#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/test/admin/{Index}", "GET")]
    public class TestOnlyReqGetAdminTestAccount : BaseDto
    {
        public string Index { get; set; }
    }
}