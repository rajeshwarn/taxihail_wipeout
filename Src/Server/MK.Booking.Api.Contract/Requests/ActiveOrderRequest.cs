using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/orders/active", "GET")]
    public class ActiveOrderRequest : IReturn<ActivateOrderResponse>
    {
    }
}
