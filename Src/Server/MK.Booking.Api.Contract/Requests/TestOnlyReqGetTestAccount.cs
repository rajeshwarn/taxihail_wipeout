#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/test/{Index}", "GET")]
    public class TestOnlyReqGetTestAccount : BaseDto
    {
        public string Index { get; set; }
    }
}