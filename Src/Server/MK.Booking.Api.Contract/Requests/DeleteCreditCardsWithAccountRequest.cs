using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/admin/deleteAllCreditCards/{AccountID}", "DELETE")]
	public class DeleteCreditCardsWithAccountRequest
	{
		public Guid AccountID { get; set; }
	}
}