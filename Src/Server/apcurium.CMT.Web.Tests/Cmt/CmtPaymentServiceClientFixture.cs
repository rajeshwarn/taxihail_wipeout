using System;
using System.Collections.Generic;
using MK.Booking.Api.Client;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common;


namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    //[Ignore("Cmt Payments - Is supposed to provide a new API - 2013-06-25")]
    public class CmtPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public CmtPaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Cmt)
        {
            
        }

        private DummyConfigManager DummyConfigManager { get; set; }

        [SetUp]
        public void Setup()
        {
            DummyConfigManager = new DummyConfigManager();
        }


        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new CmtPaymentClient(DummyConfigManager.GetPaymentSettings().CmtPaymentSettings);
        }
    }
}
