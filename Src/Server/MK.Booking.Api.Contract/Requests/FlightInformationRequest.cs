using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/flightInfo/", "POST")]
	public class FlightInformationRequest : IReturn<FlightInformation>
	{
		public DateTime Date { get; set; }

		public bool IsPickup { get; set; }

		public string AirportId { get; set; }

		public string CarrierCode { get; set; }

		public string FlightNumber { get; set; }
	}


}
