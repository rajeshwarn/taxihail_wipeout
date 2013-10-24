using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
       
    [Authenticate]
    [Route("/payment/paypal/cancel", "GET")]
    public class CancelPayPalExpressCheckoutPaymentRequest
    {
        public string Token { get; set; }
    }

}