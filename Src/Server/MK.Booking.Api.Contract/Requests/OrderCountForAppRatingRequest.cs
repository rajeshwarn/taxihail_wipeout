using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/ordercountforapprating", "GET")]
	public class OrderCountForAppRatingRequest : IReturn<int>
	{
	}
}