#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Messages
{
    [Route("/vehicle/{CarNumber}")]
    public class SendMessageToDriverRequest : IReturn<SendMessageToDriverResponse>
    {
        public string Message { get; set; }

        public string CarNumber { get; set; }
    }
}