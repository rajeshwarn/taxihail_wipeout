#region

using apcurium.MK.Common.Http;

#endregion



namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/test/{Index}", "GET")]
    public class TestOnlyReqGetTestAccount : BaseDto
    {
        public string Index { get; set; }
    }
}