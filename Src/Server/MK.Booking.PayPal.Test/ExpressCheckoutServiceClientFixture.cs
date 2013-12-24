#region

using System;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration.Impl;
using NUnit.Framework;

#endregion

namespace MK.Booking.PayPal.Test
{
    [TestFixture]
    public class ExpressCheckoutServiceClientFixture
    {
        private static ILookup<string, string> GetQueryStringLookup(Uri url)
        {
            return url
                .Query
                .TrimStart('?')
                .Split(new[] {'&'})
                .Select(x => x.Split(new[] {'='}))
                .ToLookup(x => x[0], x => x[1]);
        }

        [Test]
        [Ignore("This is a manual test. Transaction has to be authorized by a PayPal user")]
        public void DoExpressCheckoutPaymentTest()
        {
            // Arrange
            var returnUrl = "http://example.net/return";
            var cancelUrl = "http://example.net/cancel";
            var sut = new ExpressCheckoutServiceClient(new PayPalCredentials(), new RegionInfo("en-US"), true);

            var token = sut.SetExpressCheckout(12.34m, returnUrl, cancelUrl);

            // Use this checkout url to authorize the transaction (john@taxihail.com / 1234567890)
            sut.GetCheckoutUrl(token);

            // Act
            var transactionId = sut.DoExpressCheckoutPayment(token, "5CX7H5Y7VLBZQ", 12.34m);

            // Assert
            Assert.IsNotEmpty(transactionId);
        }

        [Test]
        public void GetCheckoutUrlTest()
        {
            var sut = new ExpressCheckoutServiceClient(new PayPalCredentials(), new RegionInfo("en-US"), true);

            var response = sut.GetCheckoutUrl("thetoken");
            Assert.NotNull(response);

            var checkoutUrl = new Uri(response);

            Assert.AreEqual("www.sandbox.paypal.com", checkoutUrl.Authority);
            var lookup = GetQueryStringLookup(checkoutUrl);


            Assert.AreEqual("_express-checkout-mobile", lookup["cmd"].Single());
            Assert.IsNotEmpty(lookup["token"].Single());
        }

        [Test]
        public void RegionInfo_sets_currency()
        {
            var australia = new RegionInfo("en-AU");
            var sut = new ExpressCheckoutServiceClient(new PayPalCredentials(), australia, true);

            Assert.AreEqual("AUD", sut.ISO4217CurrencySymbol);
        }

        [Test]
        public void SetExpressCheckoutTest()
        {
            var returnUrl = "http://example.net/return";
            var cancelUrl = "http://example.net/cancel";
            var sut = new ExpressCheckoutServiceClient(new PayPalCredentials(), new RegionInfo("en-US"), true);

            var token = sut.SetExpressCheckout(12.34m, returnUrl, cancelUrl);
            Assert.IsNotEmpty(token);
        }
    }
}