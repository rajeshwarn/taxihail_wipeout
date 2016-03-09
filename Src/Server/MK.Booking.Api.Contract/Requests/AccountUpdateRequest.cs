using ServiceStack.ServiceHost;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/update/{accountid}", "PUT")]
	public class AccountUpdateRequest
	{
		public Guid AccountId { get; set; }

		public BookingSettingsRequest BookingSettingsRequest { get; set; }
	}
}