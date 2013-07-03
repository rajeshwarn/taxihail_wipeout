using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/payment/paypal", "POST")]
    public class PayPalRequest : IReturn<PayPalResponse>
    {
        public decimal Amount { get; set; }
    }

    public class PayPalResponse
    {
        public string CheckoutUrl { get; set; }
    }
}