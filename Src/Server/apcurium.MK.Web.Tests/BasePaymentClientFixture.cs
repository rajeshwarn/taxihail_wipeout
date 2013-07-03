using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Booking.Api.Client;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Common;
using apcurium.MK.Web.Tests;

namespace apcurium.CMT.Web.Tests
{
    public abstract class BasePaymentClientFixture : BaseTest
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        protected BasePaymentClientFixture(TestCreditCards.TestCreditCardSetting settings)
        {
            TestCreditCards = new TestCreditCards(settings);
        }

        TestCreditCards TestCreditCards { get; set; }

        protected abstract IPaymentServiceClient GetPaymentClient();
        

        [Test]
        public void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2+"");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2+"");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();

            var response = client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate, TestCreditCards.AmericanExpress.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();

            var response = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = GetPaymentClient();


            var token = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2+"").CardOnFileToken;

            var response = client.ForgetTokenizedCard(token);
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_preauthorizing_a_credit_card_payment()
        {
            var client = GetPaymentClient();

            var token = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "").CardOnFileToken;

            const double amount = 21.56;
            var response = client.PreAuthorize(token,  amount, "orderNumber");

            Assert.AreNotEqual("-1", response);

        }

        [Test]
        public void when_capturing_a_preauthorized_a_credit_card_payment()
        {
            var client = GetPaymentClient();
            
            var token = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "").CardOnFileToken;

            const double amount = 99.50;
            const string orderNumber = "12345";
            var authorization = client.PreAuthorize(token, amount, orderNumber);

            Assert.True(authorization.IsSuccessfull, authorization.Message);

            var response = client.CommitPreAuthorized(authorization.TransactionId, orderNumber);

            Assert.True(response.IsSuccessfull, response.Message);
        }
    }
}
