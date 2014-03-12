#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/payment/paypal/success", "GET")]
    public class CompletePayPalExpressCheckoutPaymentRequest
    {
        public string Token { get; set; }
        public string PayerId { get; set; }
    }
}