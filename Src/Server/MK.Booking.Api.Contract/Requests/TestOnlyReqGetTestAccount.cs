using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/test/{Index}", "GET")]
    public class TestOnlyReqGetTestAccount : BaseDTO
    {
        public string Index { get; set; }
    }
}
