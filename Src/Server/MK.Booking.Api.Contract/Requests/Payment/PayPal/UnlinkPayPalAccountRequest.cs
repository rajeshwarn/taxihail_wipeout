using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [RouteDescription("/paypal/unlink", "POST")]
    public class UnlinkPayPalAccountRequest : IReturn<BasePaymentResponse>
    {
    }
}
