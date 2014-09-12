#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

		public async Task<string> SetExpressCheckoutForAmount(Guid orderId, decimal amount, decimal meter, decimal tip, int? ibsOrderId, string totalAmount, string languageCode)
        {
            var response = await
                Client.PostAsync(new InitiatePayPalExpressCheckoutPaymentRequest
                {
                    OrderId = orderId,
                    Amount = amount,
                    Meter = meter,
					Tip = tip,
					IbsOrderId = ibsOrderId,
                    TotalAmount = totalAmount,
					LanguageCode = languageCode
                });
            return response.CheckoutUrl;
        }
    }
}