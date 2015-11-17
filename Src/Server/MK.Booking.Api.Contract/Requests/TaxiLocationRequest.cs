using System;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[NoCache]
	[Route("/taxilocation/{OrderId}", "GET")]
	public class TaxiLocationRequest : IReturn<AvailableVehicle>
	{
		public Guid OrderId { get; set; }
		public string Medallion { get; set; }
	}
}
