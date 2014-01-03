#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public string SetExpressCheckoutForAmount(Guid orderId, decimal amount, decimal meter, decimal tip)
        {
            var response =
                Client.Post(new InitiatePayPalExpressCheckoutPaymentRequest
                {
                    OrderId = orderId,
                    Amount = amount,
                    Meter = meter,
                    Tip = tip
                });
            return response.CheckoutUrl;
        }
    }
}