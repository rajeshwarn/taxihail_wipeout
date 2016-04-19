using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/vehicle/{VehicleNumber}/message", "POST")]
	public class SendMessageToDriverRequest : BaseDto
	{
		public string Message { get; set; }

	    public Guid OrderId { get; set; }

	    public string VehicleNumber { get; set; }
	}
}