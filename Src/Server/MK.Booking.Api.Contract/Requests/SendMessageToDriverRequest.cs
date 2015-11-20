using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/vehicle/{VehicleNumber}/message", "POST")]
	public class SendMessageToDriverRequest : BaseDto
	{
		public string Message { get; set; }

	    public Guid OrderId { get; set; }

	    public string VehicleNumber { get; set; }
	}
}