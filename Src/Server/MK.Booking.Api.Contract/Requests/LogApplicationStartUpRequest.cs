using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[Route("/account/logstartup", "POST")]
	public class LogApplicationStartUpRequest
	{
        public DateTime StartUpDate { get; set; }

        public string ApplicationVersion { get; set; }

        public string Platform { get; set; }

        public string PlatformDetails { get; set; }
	}
}

