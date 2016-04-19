using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/account/findaccounts/{searchCriteria}", "GET")]
	public class FindAccountsRequest
	{
		public string SearchCriteria { get; set; }
	}
}