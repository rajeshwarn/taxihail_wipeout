using System;
using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
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
            return new BraintreeServiceClient(BaseUrl, SessionId, new BraintreeClientSettings().ClientKey, new DummyPackageInfo(), null);
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
        public override async void when_settling_an_overdue_payment()
        {
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            var pickUpDate = DateTime.Now;

            var client = GetPaymentClient();

            using (var context = ContextFactory.Invoke())
            {
                context.RemoveAll<OrderDetail>();
                context.RemoveAll<OverduePaymentDetail>();
                context.SaveChanges();

                var braintreeGateway = BraintreePaymentService.GetBraintreeGateway(new BraintreeServerSettings());

                var encryptedCard = ((BraintreeServiceClient)client).EncryptCreditCard(TestCreditCards.Visa.Number,
                    TestCreditCards.Visa.ExpirationDate,
                    TestCreditCards.Visa.AvcCvvCvv2.ToString());

                var customerResult = braintreeGateway.Customer.Create(new CustomerRequest());

                var creditCard = braintreeGateway.CreditCard.Create(new CreditCardRequest
                {
                    Number = encryptedCard[0],
                    ExpirationDate = encryptedCard[1],
                    CVV = encryptedCard[2],
                    CustomerId = customerResult.Target.Id,
                    Options = new CreditCardOptionsRequest
                    {
                        VerifyCard = true
                    }
                });

                var token = creditCard.Target.Token;

                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);
                testAccount.DefaultCreditCard = creditCardId;

                context.RemoveAll<CreditCardDetails>();
                context.SaveChanges();

                context.Set<CreditCardDetails>().Add(new CreditCardDetails
                {
                    CreditCardId = creditCardId,
                    AccountId = TestAccount.Id,
                    CreditCardCompany = "Visa",
                    Token = token
                });

                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    AccountId = TestAccount.Id,
                    BookingFees = 15m,
                    CreatedDate = DateTime.Now,
                    PickupDate = pickUpDate,
                    PickupAddress = TestAddresses.GetAddress1(),
                    ClientLanguageCode = SupportedLanguages.en.ToString()
                });

                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    IBSOrderId = 12345,
                    VehicleNumber = "9001",
                    Status = OrderStatus.Canceled,
                    AccountId = TestAccount.Id,
                    PickupDate = pickUpDate
                });

                context.Set<OrderPairingDetail>().Add(new OrderPairingDetail
                {
                    OrderId = orderId,
                    AutoTipPercentage = 15
                });

                context.Set<OverduePaymentDetail>().Add(new OverduePaymentDetail
                {
                    AccountId = TestAccount.Id,
                    IBSOrderId = 12345,
                    OrderId = orderId,
                    TransactionDate = DateTime.Now,
                    TransactionId = "TransId",
                    OverdueAmount = 52.34m,
                    ContainBookingFees = false,
                    ContainStandaloneFees = false,
                    IsPaid = false
                });

                context.SaveChanges();
            }

            var result = await client.SettleOverduePayment();
            Assert.AreEqual(true, result.IsSuccessful);

            var overduePayment = await client.GetOverduePayment();
            Assert.IsNull(overduePayment);
        }

        [Test]
        public override async void when_deleting_a_tokenized_credit_card()
        {
            var client = GetPaymentClient();

            string token;
            using (var context = ContextFactory.Invoke())
            {
                var braintreeGateway = BraintreePaymentService.GetBraintreeGateway(new BraintreeServerSettings());

                var encryptedCard = ((BraintreeServiceClient)client).EncryptCreditCard(TestCreditCards.Visa.Number,
                    TestCreditCards.Visa.ExpirationDate,
                    TestCreditCards.Visa.AvcCvvCvv2.ToString());

                var creditCardId = Guid.NewGuid();

                var customerResult = braintreeGateway.Customer.Create(new CustomerRequest());

                var creditCard = braintreeGateway.CreditCard.Create(new CreditCardRequest
                {
                    Number = encryptedCard[0],
                    ExpirationDate = encryptedCard[1],
                    CVV = encryptedCard[2],
                    CustomerId = customerResult.Target.Id,
                    Options = new CreditCardOptionsRequest
                    {
                        VerifyCard = true
                    }
                });

                token = creditCard.Target.Token;

                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);
                testAccount.DefaultCreditCard = creditCardId;

                context.RemoveAll<CreditCardDetails>();
                context.SaveChanges();

                context.Set<CreditCardDetails>().Add(new CreditCardDetails
                {
                    CreditCardId = creditCardId,
                    AccountId = TestAccount.Id,
                    CreditCardCompany = "Visa",
                    Token = token
                });
            }

            var response = await client.ForgetTokenizedCard(token);
            Assert.True(response.IsSuccessful, response.Message);
        }

        [Test]
        public async void when_providing_paypal_nounce()
        {
            var client = GetPaymentClient();
            var response = await client.AddPaymentMethod("fake-paypal-future-nonce", PaymentMethods.Paypal, null);
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public async void when_fetching_client_token()
        {
            var client = GetPaymentClient();
            var response = await client.GenerateClientTokenResponse();
            Assert.NotNull(response.ClientToken);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();
            var response = await client.AddPaymentMethod("fake-valid-amex-nonce", PaymentMethods.CreditCard, null);
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();
            var response = await client.AddPaymentMethod("fake-valid-discover-nonce", PaymentMethods.CreditCard, null);
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = await client.AddPaymentMethod("fake-valid-mastercard-nonce", PaymentMethods.CreditCard, null);
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }

        [Test]
        public override async void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = await client.AddPaymentMethod("fake-valid-visa-nonce", PaymentMethods.CreditCard, null);
            Assert.True(response.IsSuccessful, response.Message);
            Assert.NotNull(response.BraintreeAccountId);
        }
    }
}