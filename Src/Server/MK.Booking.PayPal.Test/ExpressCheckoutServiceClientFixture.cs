using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace MK.Booking.PayPal.Test
{
    [TestFixture]
    public class ExpressCheckoutServiceClientFixture
    {
        [Test]
        public void RegionInfo_sets_currency()
        {
            var australia = new RegionInfo("en-AU");
            var sut = new ExpressCheckoutServiceClient(new PayPalSandboxCredentials(), australia, "www", "www", useSandbox: true);

            Assert.AreEqual("AUD", sut.ISO4217CurrencySymbol);


        }

        [Test]
        public void SetExpressCheckoutTest()
        {
            var returnUrl = "http://example.net/return";
            var cancelUrl = "http://example.net/cancel";
            var sut = new ExpressCheckoutServiceClient(new PayPalSandboxCredentials(), new RegionInfo("en-US"), returnUrl, cancelUrl, useSandbox: true);

            var response = sut.SetExpressCheckout(12.34m);
            Assert.NotNull(response);

            var checkoutUrl = new Uri(response);

            Assert.AreEqual("www.paypal.com", checkoutUrl.Authority);
            var lookup = GetQueryStringLookup(checkoutUrl);


            Assert.AreEqual("_express-checkout-mobile", lookup["cmd"].Single());
            Assert.IsNotEmpty(lookup["token"].Single());

        }

        static ILookup<string, string> GetQueryStringLookup(Uri url)
        {
            return url
                .Query
                .TrimStart('?')
                .Split(new[] {'&'})
                .Select(x => x.Split(new[] {'='}))
                .ToLookup(x => x[0], x => x[1]);
        }
    }
}
