using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payment/paypal/success", "GET")]
    public class CompletePayPalExpressCheckoutPaymentRequest
    {
        public string Token { get; set; }
        public string PayerId { get; set; }
    }

}