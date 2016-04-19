using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/accounts/logstartup", "POST")]
	public class LogApplicationStartUpRequest
	{
        public DateTime StartUpDate { get; set; }

        public string ApplicationVersion { get; set; }

        public string Platform { get; set; }

        public string PlatformDetails { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
	}
}

