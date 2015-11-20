using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/settleoverduepayment", "POST")]
    public class SettleOverduePaymentRequest : IReturn<SettleOverduePaymentResponse>
    {
    }
}