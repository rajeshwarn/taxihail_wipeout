using MK.Booking.PayPal;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Payment;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService: Service
    {
        readonly ExpressCheckoutServiceClient _client;

        public PayPalService(ExpressCheckoutServiceFactory factory)
        {
            _client = factory.CreateService();
        }

        public PayPalResponse Post(PayPalRequest request)
        {
            var checkoutUrl = _client.SetExpressCheckout(request.Amount);

            return new PayPalResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }
    }
}
