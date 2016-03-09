using System;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[NoCache]
	[Route("/taxilocation/{OrderId}", "GET")]
	public class TaxiLocationRequest : IReturn<AvailableVehicle>
	{
		public Guid OrderId { get; set; }
		public string Medallion { get; set; }
	}
}
