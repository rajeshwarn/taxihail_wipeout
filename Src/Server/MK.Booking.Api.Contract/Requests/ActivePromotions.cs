using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/promotions/", "GET")]
    public class ActivePromotions : IReturn<ActivePromotion[]>
    {
    }
}