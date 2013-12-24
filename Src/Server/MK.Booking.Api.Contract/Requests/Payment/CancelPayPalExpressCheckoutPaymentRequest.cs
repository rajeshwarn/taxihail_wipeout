#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/payment/paypal/cancel", "GET")]
    public class CancelPayPalExpressCheckoutPaymentRequest
    {
        public string Token { get; set; }
    }
}