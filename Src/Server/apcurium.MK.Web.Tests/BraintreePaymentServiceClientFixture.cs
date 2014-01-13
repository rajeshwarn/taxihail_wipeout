using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class BraintreePaymentServiceClientFixture : BasePaymentClientFixture
    {
        public BraintreePaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Braintree)
        {
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new BraintreeServiceClient(BaseUrl, SessionId, new BraintreeClientSettings().ClientKey, "Test");
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Braintree;
        }
    }
}