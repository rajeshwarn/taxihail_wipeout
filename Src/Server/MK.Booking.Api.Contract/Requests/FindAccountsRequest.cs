using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/findaccounts/{searchCriteria}", "GET")]
	public class FindAccountsRequest
	{
		public string SearchCriteria { get; set; }
	}
}