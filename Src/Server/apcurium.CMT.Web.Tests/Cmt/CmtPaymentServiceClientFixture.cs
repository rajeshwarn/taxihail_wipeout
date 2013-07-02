using System;
using System.Collections.Generic;
using MK.Booking.Api.Client;
using NUnit.Framework;
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
        const string MERCHANT_TOKEN = "E4AFE87B0E864228200FA947C4A5A5F98E02AA7A3CFE907B0AD33B56D61D2D13E0A75F51641AB031500BD3C5BDACC114";
        const string CONSUMER_KEY = "vmAoqWEY3zIvUCM4";
        const string CONSUMER_SECRET_KEY = "DUWzh0jAldPc7C5I";
        const string SANDBOX_BASE_URL = "https://payment-sandbox.cmtapi.com/v2/merchants/" + MERCHANT_TOKEN + "/";
        const string BASE_URL = SANDBOX_BASE_URL; // for now will will not use production	


        [SetUp]
        public void Setup()
        {
            DummyConfigManager = new DummyConfigManager();
        }


        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new CmtPaymentClient(BASE_URL, CONSUMER_KEY, CONSUMER_SECRET_KEY, AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar, true);
        }
    }
}
