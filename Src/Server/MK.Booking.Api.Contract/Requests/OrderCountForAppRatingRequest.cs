using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/account/ordercountforapprating", "GET")]
	public class OrderCountForAppRatingRequest : IReturn<int>
	{
	}
}