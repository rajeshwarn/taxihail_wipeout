using System;
using System.Linq;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Braintree;
using CreditCard = apcurium.MK.Common.CreditCard;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class BraintreePaymentServiceClientFixture : BasePaymentClientFixture
    {
        public BraintreePaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Braintree)
        {
        }

        public override void Setup()
        {
            base.Setup();
            var paymentService = GetPaymentService();
            UnityServiceLocator.Instance.RegisterInstance<IPaymentService>(paymentService);
            


        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new BraintreeServiceClient(BaseUrl, SessionId, new BraintreeClientSettings().ClientKey, new DummyPackageInfo());
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Braintree;
        }

        private IPaymentService GetPaymentService()
        {
            var commandBus = UnityServiceLocator.Instance.Resolve<ICommandBus>();
            var logger = UnityServiceLocator.Instance.Resolve<ILogger>();
            var orderPaymentDao = UnityServiceLocator.Instance.Resolve<IOrderPaymentDao>();
            var serverSettings = UnityServiceLocator.Instance.Resolve<IServerSettings>();
            var pairingService = UnityServiceLocator.Instance.Resolve<IPairingService>();
            var creditCardDao = UnityServiceLocator.Instance.Resolve<ICreditCardDao>();
            return new BraintreePaymentService(commandBus, logger, orderPaymentDao, serverSettings, serverSettings.GetPaymentSettings(), pairingService, creditCardDao);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_visa_with_existing_credit_card()
        {
            string customerId;

            using (var context = ContextFactory.Invoke())
            {
                var braintreeGateway = BraintreePaymentService.GetBraintreeGateway(new BraintreeServerSettings());

                var encryptedCard = ((BraintreeServiceClient) GetPaymentClient()).EncryptCreditCard(TestCreditCards.Visa.Number,
                        TestCreditCards.Visa.ExpirationDate, 
                        TestCreditCards.Visa.AvcCvvCvv2.ToString());


                var result = braintreeGateway.Customer.Create(new CustomerRequest()
                {
                    CreditCard = new CreditCardRequest
                    {
                        Number = encryptedCard[0],
                        ExpirationDate = encryptedCard[1],
                        CVV = encryptedCard[2],
                        Options = new CreditCardOptionsRequest
                        {
                            VerifyCard = true
                        }
                    }
                });

                if (result.Errors != null)
                {
                    Assert.Fail(result.Message);
                }

                var customer = result.Target;
                customerId = customer.Id;
                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);
                
                var creditCardToken = customer.CreditCards.First();
                
                var creditCardId = Guid.NewGuid();

                context.Set<CreditCardDetails>().Add(new CreditCardDetails()
                {
                    Token = creditCardToken.Token,
                    AccountId = TestAccount.Id,
                    CreditCardId = creditCardId
                });

                testAccount.BraintreeAccountId = customerId;
                testAccount.DefaultCreditCard = creditCardId;
                
                context.SaveChanges();
            }

            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
            Assert.AreEqual(customerId, response.BraintreeAccountId);
        }



        [Test]
        public async void when_tokenizing_a_credit_card_visa_with_existing_customer_id()
        {
            using (var context = ContextFactory.Invoke())
            {
                var braintreeGateway = BraintreePaymentService.GetBraintreeGateway(new BraintreeServerSettings());

                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);

                var result = braintreeGateway.Customer.Create();

                var customer = result.Target;

                testAccount.BraintreeAccountId = customer.Id;

                context.SaveChanges();

                var client = GetPaymentClient();
                var response = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
                Assert.True(response.IsSuccessful, response.Message);
                Assert.NotNull(response.BraintreeAccountId);
                Assert.AreEqual(customer.Id, response.BraintreeAccountId);
            }
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate, TestCreditCards.AmericanExpress.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }
    }
}