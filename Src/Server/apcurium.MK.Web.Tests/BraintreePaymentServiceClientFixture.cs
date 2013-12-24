using apcurium.CMT.Web.Tests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Web.Tests
{
    public class BraintreePaymentServiceClientFixture :BasePaymentClientFixture
    {
        public BraintreePaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Braintree)
        {}

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new BraintreeServiceClient(BaseUrl, SessionId, new BraintreeClientSettings().ClientKey, "Test");
        }

        protected override Common.Enumeration.PaymentProvider GetProvider()
        {
            return Common.Enumeration.PaymentProvider.Braintree;
        }
    }
}
