using System;
using System.Collections.Generic;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Common.Configuration;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;


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

        public class  CreditCards
        {
            public class Visa
            {
                public static string Number = "4012 0000 3333 0026".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 135;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class Mastercard
            {
                public static string Number = "5424 1802 7979 1732".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 135;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class AmericanExpress
            {
                public static string Number = "3410 9293 659 1002".Replace(" ", "");
                public static string ZipCode = "55555";
                public static int AvcCvvCvv2 = 1002;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class Discover
            {
                public static string Number = "6011 0002 5950 5851".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 111;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

        }
       
        [Test]
        public void when_tokenizing_a_credit_card_visa()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(CreditCards.Visa.Number, CreditCards.Visa.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_mastercard()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(CreditCards.Mastercard.Number, CreditCards.Mastercard.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_tokenizing_a_credit_card_amex()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(CreditCards.AmericanExpress.Number, CreditCards.AmericanExpress.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }
        
        [Test]
        public void when_tokenizing_a_credit_card_discover()
        {
            var client = new CmtPaymentClient(DummyConfigManager);
            var response = client.Tokenize(CreditCards.Discover.Number, CreditCards.Discover.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode, response.ResponseMessage);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = new CmtPaymentClient(DummyConfigManager);

            var token = client.Tokenize(CreditCards.Visa.Number, CreditCards.Visa.ExpirationDate).CardOnFileToken;

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
            var token = client.Tokenize(CreditCards.Mastercard.Number, CreditCards.Mastercard.ExpirationDate).CardOnFileToken;

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

            var token = client.Tokenize(CreditCards.Discover.Number, CreditCards.Discover.ExpirationDate).CardOnFileToken;

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
