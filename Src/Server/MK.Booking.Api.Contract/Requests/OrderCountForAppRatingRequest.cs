using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
    [Route("/account/ordercountforapprating", "GET")]
	public class OrderCountForAppRatingRequest : IReturn<int>
	{
	}
}