using ServiceStack.ServiceHost;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/admin/deleteAllCreditCards/{AccountID}", "DELETE")]
	public class DeleteCreditCardsWithAccountRequest
	{
		public Guid AccountID { get; set; }
	}
}