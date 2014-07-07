using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[Route("/account/logstartup", "POST")]
	public class LogApplicationStartUpRequest
	{
		public DateTime StartUpDate;

		public string ApplicationVersion;

		public string Platform;

		public string PlatformDetails;

		public string User;
	}
}

