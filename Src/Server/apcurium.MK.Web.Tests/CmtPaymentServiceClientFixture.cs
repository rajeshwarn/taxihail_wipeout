using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CmtPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public CmtPaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Cmt)
        {
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new CmtPaymentClient(BaseUrl, SessionId, new CmtPaymentSettings(), "Test");
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Cmt;
        }
    }
}