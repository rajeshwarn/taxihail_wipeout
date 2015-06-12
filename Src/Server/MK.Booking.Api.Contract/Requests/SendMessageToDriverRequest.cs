using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("driver/{VehicleNumber}", "POST")]
	public class SendMessageToDriverRequest : BaseDto
	{
		public string Message { get; set; }


	    public string VehicleNumber { get; set; }
	}
}