using System;
using System.Collections.Generic;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common;


namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class CmtPaymentServiceClientFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
        }

        [SetUp]
        public void Setup()
        {

            


        }



        private CmtPaymentClient GetCmtPaymentClient()
        {

            string MERCHANT_TOKEN = "E4AFE87B0E864228200FA947C4A5A5F98E02AA7A3CFE907B0AD33B56D61D2D13E0A75F51641AB031500BD3C5BDACC114";
            string CONSUMER_KEY = "vmAoqWEY3zIvUCM4";
            string CONSUMER_SECRET_KEY = "DUWzh0jAldPc7C5I";
            string SANDBOX_BASE_URL = "https://payment-sandbox.cmtapi.com/v2/merchants/" + MERCHANT_TOKEN + "/";
            string BASE_URL = SANDBOX_BASE_URL; // for now will will not use production		
            return new CmtPaymentClient(BASE_URL, CONSUMER_KEY, CONSUMER_SECRET_KEY, AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar, true);
        }


        [Test]
        public void when_tokenizing_a_credit_card_visa()
        {
            var client = GetCmtPaymentClient();
            var response = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetCmtPaymentClient();
            var response = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_amex()
        {
            var client = GetCmtPaymentClient();

            var response = client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_discover()
        {
            var client = GetCmtPaymentClient();

            var response = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = GetCmtPaymentClient();


            var token = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate).CardOnFileToken;

            var response = client.ForgetTokenizedCard(token);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }
        //[Test]
        //public void when_preauthorizing_a_credit_card_payment_usd()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
        //                           AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar);

        //    when_preauthorizing_a_credit_card_payment();
        //}

        //[Test]
        //public void when_preauthorizing_a_credit_card_payment_cad()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
        //                           AuthorizationRequest.CurrencyCodes.Main.CanadaDollar);

        //    when_preauthorizing_a_credit_card_payment();

        //}

        //[Test]
        //public void when_preauthorizing_a_credit_card_payment_eurp()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
        //                           AuthorizationRequest.CurrencyCodes.Main.EuroMemberCountries);

        //    when_preauthorizing_a_credit_card_payment();
        //}

        private void when_preauthorizing_a_credit_card_payment()
        {
            var client = GetCmtPaymentClient();

            var token = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate).CardOnFileToken;

            const double amount = 21.56;
            var response = client.PreAuthorizeTransaction(token, amount, "orderNumber");

            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);

            Assert.AreEqual(amount, response.Amount, .001);
            //if (response.CurrencyCode !=
            //    DummyConfigManager.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString))
            //{
            //    Assert.Inconclusive("Currency Codes dont match");
            //}
        }

        //[Test]
        //public void when_capturing_a_preauthorized_a_credit_card_payment_euro()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
        //   AuthorizationRequest.CurrencyCodes.Main.EuroMemberCountries);

        //    when_capturing_a_preauthorized_a_credit_card_payment();
        //}
        //[Test]
        //public void when_capturing_a_preauthorized_a_credit_card_payment_usd()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
        //   AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar);

        //    when_capturing_a_preauthorized_a_credit_card_payment();
        //}
        //[Test]
        //public void when_capturing_a_preauthorized_a_credit_card_payment_cad()
        //{
        //    DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString, AuthorizationRequest.CurrencyCodes.Main.CanadaDollar);

        //    when_capturing_a_preauthorized_a_credit_card_payment();
        //}

        private void when_capturing_a_preauthorized_a_credit_card_payment()
        {
            
            var client = GetCmtPaymentClient();


            var token = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate).CardOnFileToken;

            const double amount = 99.50;
            const string orderNumber = "12345";
            var authorization = client.PreAuthorizeTransaction(token, amount, orderNumber);

            var response = client.CapturePreAuthorized(authorization.TransactionId, orderNumber);

            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
            Assert.AreEqual(amount, response.Amount, .001);
            //if (response.CurrencyCode != DummyConfigManager.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString))
            //{
            //    Assert.Inconclusive("Currency Codes dont match");
            //}
        }



    }
}
