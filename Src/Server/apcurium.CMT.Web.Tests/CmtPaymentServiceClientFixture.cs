using System;
using System.Collections.Generic;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Tests;


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

            DummyConfigManager = new DummyConfigManager(
                new Dictionary<string, string>()
                    {
                        {
                            AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
                            AuthorizationRequest.CurrencyCodes.Main.EuroMemberCountries
                        }
                    });
    

        }

        protected DummyConfigManager DummyConfigManager { get; set; }

        
       
        [Test]
        public void when_tokenizing_a_credit_card_visa()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_mastercard()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_amex()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }
        
        [Test]
        public void when_tokenizing_a_credit_card_discover()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = new CmtPaymentClient(DummyConfigManager);

            var token = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate).CardOnFileToken;

            var response = client.ForgetTokenizedCard(token);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }
        [Test]
        public void when_preauthorizing_a_credit_card_payment_usd()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
                                   AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar);
            
            when_preauthorizing_a_credit_card_payment();
        }

        [Test]
        public void when_preauthorizing_a_credit_card_payment_cad()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
                                   AuthorizationRequest.CurrencyCodes.Main.CanadaDollar);

            when_preauthorizing_a_credit_card_payment();
            
        }

        [Test]
        public void when_preauthorizing_a_credit_card_payment_eurp()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
                                   AuthorizationRequest.CurrencyCodes.Main.EuroMemberCountries);

            when_preauthorizing_a_credit_card_payment();
        }

        private void when_preauthorizing_a_credit_card_payment()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var token = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate).CardOnFileToken;

            const double amount = 21.56;
            var response = client.PreAuthorizeTransaction(token, amount);

            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);

            Assert.AreEqual(amount, response.Amount, .001);
            if (response.CurrencyCode !=
                DummyConfigManager.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString))
            {
                Assert.Inconclusive("Currency Codes dont match");
            }
        }

        [Test]
        public void when_capturing_a_preauthorized_a_credit_card_payment_euro()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
           AuthorizationRequest.CurrencyCodes.Main.EuroMemberCountries);

            when_capturing_a_preauthorized_a_credit_card_payment();
        }
        [Test]
        public void when_capturing_a_preauthorized_a_credit_card_payment_usd()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
           AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar);

            when_capturing_a_preauthorized_a_credit_card_payment();
        }
        [Test]
        public void when_capturing_a_preauthorized_a_credit_card_payment_cad()
        {
            DummyConfigManager.AddOrSet(AuthorizationRequest.CurrencyCodes.CurrencyCodeString,
           AuthorizationRequest.CurrencyCodes.Main.CanadaDollar);

            when_capturing_a_preauthorized_a_credit_card_payment();
        }

        private void when_capturing_a_preauthorized_a_credit_card_payment()
        {
            var client = new CmtPaymentClient(DummyConfigManager);

            var token = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate).CardOnFileToken;

            const double amount = 99.50;
            var authorization = client.PreAuthorizeTransaction(token, amount);

            var response = client.CapturePreAuthorized(authorization.TransactionId);

            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
            Assert.AreEqual(amount, response.Amount, .001);
            if (response.CurrencyCode != DummyConfigManager.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString))
            {
                Assert.Inconclusive("Currency Codes dont match");
            }
        }



    }
}
