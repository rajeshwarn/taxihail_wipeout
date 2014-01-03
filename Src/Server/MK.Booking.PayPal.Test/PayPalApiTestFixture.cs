#region

using NUnit.Framework;

#endregion

namespace MK.Booking.PayPal.Test
{
    [TestFixture]
    public class PayPalApiTestFixture
    {
        [Test]
        public void SetExpressCheckoutTest()
        {
            var credentials = new UserIdPasswordType
            {
                Username = "vincent.costel-facilitator_api1.gmail.com",
                Password = "1372362468",
                Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ADYXGX.gPsewqg6pNBBa9JL5zoCL",
            };
            var securityHeader = new CustomSecurityHeaderType
            {
                Credentials = credentials
            };
            var api = new PayPalAPIAASoapBinding
            {
                Url = "https://api-3t.sandbox.paypal.com/2.0/",
                RequesterCredentials = securityHeader,
            };

            var orderTotal = new BasicAmountType
            {
                Value = "10.00",
                currencyID = CurrencyCodeType.USD
            };
            var requestDetails = new SetExpressCheckoutRequestDetailsType
            {
                OrderTotal = orderTotal,
                PaymentAction = PaymentActionCodeType.Sale,
                ReturnURL = "http://www.google.com",
                CancelURL = "http://www.google.com"
            };
            var requestType = new SetExpressCheckoutRequestType
            {
                Version = "104",
                SetExpressCheckoutRequestDetails = requestDetails,
            };

            var request = new SetExpressCheckoutReq
            {
                SetExpressCheckoutRequest = requestType,
            };

            var response = api.SetExpressCheckout(request);

            Assert.AreEqual(AckCodeType.Success, response.Ack);
            Assert.NotNull(response.Token);
        }
    }
}