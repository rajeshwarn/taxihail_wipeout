using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/account/update/{accountid}", "PUT")]
	public class AccountUpdateRequest
	{
		public Guid AccountId { get; set; }

		public BookingSettingsRequest BookingSettingsRequest { get; set; }
	}
}