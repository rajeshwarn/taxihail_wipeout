using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/settleoverduepayment", "POST")]
    public class SettleOverduePaymentRequest : IReturn<SettleOverduePaymentResponse>
    {
    }
}