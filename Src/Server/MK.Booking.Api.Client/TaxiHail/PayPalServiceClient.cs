#region

using System;
using System.Threading.Tasks;
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

        public async Task<string> SetExpressCheckoutForAmount(Guid orderId, decimal amount, decimal meter, decimal tip)
        {
            var response = await
                Client.PostAsync(new InitiatePayPalExpressCheckoutPaymentRequest
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