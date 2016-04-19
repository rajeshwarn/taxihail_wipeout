using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [RouteDescription("/paypal/link", "POST")]
    public class LinkPayPalAccountRequest : IReturn<BasePaymentResponse>
    {
        public string AuthCode { get; set; }
    }
}