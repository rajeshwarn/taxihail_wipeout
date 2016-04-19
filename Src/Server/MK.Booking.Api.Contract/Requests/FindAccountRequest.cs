using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/account/findaccount/{accountid}", "GET")]
	public class FindAccountRequest
	{
		public Guid AccountId { get; set; }
	}
}