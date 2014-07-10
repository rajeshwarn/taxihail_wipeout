using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class MonerisPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public MonerisPaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Moneris)
        {
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new MonerisServiceClient(BaseUrl, SessionId, new MonerisPaymentSettings(), new DummyPackageInfo(), new Logger());
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Moneris;
        }
    }
}