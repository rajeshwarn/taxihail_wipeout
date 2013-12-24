using NUnit.Framework;
using apcurium.CMT.Web.Tests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    //[Ignore("Cmt Payments - Is supposed to provide a new API - 2013-06-25")]
    public class CmtPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public CmtPaymentServiceClientFixture()
            : base(TestCreditCards.TestCreditCardSetting.Cmt)
        {

        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new CmtPaymentClient(BaseUrl, SessionId, new CmtPaymentSettings(), new Logger(), "Test");
        }

        protected override Common.Enumeration.PaymentProvider GetProvider()
        {
            return Common.Enumeration.PaymentProvider.Cmt;
        }
    }
}
