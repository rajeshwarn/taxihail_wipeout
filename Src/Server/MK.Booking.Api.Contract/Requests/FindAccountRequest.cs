using ServiceStack.ServiceHost;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/findaccount/{accountid}", "GET")]
	public class FindAccountRequest
	{
		public Guid AccountId { get; set; }
	}
}