using System;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/taxilocation/{OrderId}", "GET")]
	public class TaxiLocationRequest : IReturn<AvailableVehicle>
	{
		public Guid OrderId { get; set; }
		public string Medallion { get; set; }
	}
}
