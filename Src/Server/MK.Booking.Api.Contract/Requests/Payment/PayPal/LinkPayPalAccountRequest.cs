using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [Route("/paypal/link", "POST")]
    public class LinkPayPalAccountRequest : IReturn<BasePaymentResponse>
    {
        public string AuthCode { get; set; }
    }
}