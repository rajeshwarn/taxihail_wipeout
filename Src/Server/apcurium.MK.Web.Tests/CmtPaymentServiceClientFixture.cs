#region

using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

#endregion

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
            return new CmtPaymentClient(BaseUrl, SessionId, new CmtPaymentSettings(), "Test");
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Cmt;
        }
    }
}