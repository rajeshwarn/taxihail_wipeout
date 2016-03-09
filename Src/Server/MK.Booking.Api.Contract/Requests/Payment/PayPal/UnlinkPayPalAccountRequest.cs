using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [Route("/paypal/unlink", "POST")]
    public class UnlinkPayPalAccountRequest : IReturn<BasePaymentResponse>
    {
    }
}
