using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/flightInfo/", "POST")]
	public class FlightInformationRequest : IReturn<FlightInformation>
	{
		public DateTime Date { get; set; }

		public bool IsPickup { get; set; }

		public string AirportId { get; set; }

		public string CarrierCode { get; set; }

		public string FlightNumber { get; set; }
	}


}
