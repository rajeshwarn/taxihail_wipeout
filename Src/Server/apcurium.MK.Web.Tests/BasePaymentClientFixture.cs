using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public abstract class BasePaymentClientFixture : BaseTest
    {
        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();
            await CreateAndAuthenticateTestAccount();
        }

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        protected BasePaymentClientFixture(TestCreditCards.TestCreditCardSetting settings)
        {
            TestCreditCards = new TestCreditCards(settings);
            var connectionString = ConfigurationManager.ConnectionStrings["MKWebDev"].ConnectionString;
            ContextFactory = () => new BookingDbContext(connectionString);
        }

        protected Func<DbContext> ContextFactory { get; set; }

        private TestCreditCards TestCreditCards { get; set; }

        protected abstract IPaymentServiceClient GetPaymentClient();
        protected abstract PaymentProvider GetProvider();

        [Test]
        public async void when_authorized_a_credit_card_payment_and_resending_confirmation()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleNumber = "vehicle",
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var tokenClient = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            var token = tokenClient.CardOnFileToken;

            const double amount = 12.75;
            const double meter = 11.25;
            const double tip = 1.50;

            var authorization = await client.PreAuthorize(token, amount, meter, tip, orderId);
            Assert.True(authorization.IsSuccessfull, authorization.Message);

            var response = await client.CommitPreAuthorized(authorization.TransactionId);
            Assert.True(response.IsSuccessfull, response.Message);

            await client.ResendConfirmationToDriver(orderId);
            
            using (var context = ContextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().Single(p => p.OrderId == orderId);

                Assert.AreEqual(amount, payment.Amount);
                Assert.AreEqual(meter, payment.Meter);
                Assert.AreEqual(tip, payment.Tip);

                Assert.AreEqual(PaymentType.CreditCard, payment.Type);
                Assert.AreEqual(GetProvider(), payment.Provider);
            }
        }

        [Test]
        public async void when_capturing_a_preauthorized_a_credit_card_payment()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleNumber = "vehicle",
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var tokenizeResponse = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            const double amount = 22.75;
            const double meter = 21.25;
            const double tip = 1.25;

            var authorization = await client.PreAuthorize(token, amount, meter, tip, orderId);
            Assert.True(authorization.IsSuccessfull, authorization.Message);

            var response = await client.CommitPreAuthorized(authorization.TransactionId);
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public async void when_deleting_a_tokenized_credit_card()
        {
            var client = GetPaymentClient();
            
            var tokenizeResponse = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            var response = await client.ForgetTokenizedCard(token);
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public async void when_preauthorizing_a_credit_card_payment()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.SaveChanges();
            }
            var client = GetPaymentClient();

            var tokenizeResponse = await client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            const double amount = 22.75;
            const double meter = 21.25;
            const double tip = 1.25;

            var response = await client.PreAuthorize(token, amount, meter, tip, orderId);
            Assert.True(response.IsSuccessfull);
        }

        [Test]
        public async void when_preauthorizing_and_capturing_a_credit_card_payment()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleNumber = "vehicle",
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var tokenizeResponse = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            const double amount = 31.50;
            const double meter = 21.25;
            const double tip = 10.25;

            var authorization = await client.PreAuthorizeAndCommit(token, amount, meter, tip, orderId);
            Assert.True(authorization.IsSuccessfull, authorization.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate, TestCreditCards.AmericanExpress.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }
    }
}