using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/orders/active", "GET")]
    [Authenticate]
    public class ActiveOrderRequest : IReturn<ActivateOrderResponse>
    {
    }
}
