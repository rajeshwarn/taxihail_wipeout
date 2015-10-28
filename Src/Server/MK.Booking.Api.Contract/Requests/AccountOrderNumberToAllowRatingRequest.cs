using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[Route("/account/ordernumbertoallowrating", "GET")]
	public class AccountOrderNumberToAllowRatingRequest:IReturn<int>
	{
	}
}