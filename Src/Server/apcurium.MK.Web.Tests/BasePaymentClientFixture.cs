using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public abstract class BasePaymentClientFixture : BaseTest
    {
        private Type _ibsImplementation;
        private FakeIbs _fakeIbs;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            //CreateAndAuthenticateTestAccount().Wait();
            //replace it with a fake
            _fakeIbs = new FakeIbs();
            UnityServiceLocator.Instance.RegisterInstance<IIbsOrderService>(_fakeIbs);
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
            var container = UnityServiceLocator.Instance;
            _ibsImplementation = container.Registrations
                                            .FirstOrDefault(x => x.RegisteredType == typeof(IIbsOrderService))
                                            .MappedToType;
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
            UnityServiceLocator.Instance.RegisterType(typeof(IIbsOrderService), _ibsImplementation);
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
        public async void when_getting_an_overdue_payment()
        {
            var orderId = Guid.NewGuid();

            using (var context = ContextFactory.Invoke())
            {
                context.RemoveAll<OverduePaymentDetail>();
                context.SaveChanges();

                context.Set<OverduePaymentDetail>().Add(new OverduePaymentDetail
                {
                    AccountId = TestAccount.Id,
                    IBSOrderId = 12345,
                    OrderId = orderId,
                    TransactionDate = DateTime.Now,
                    TransactionId = "TransId",
                    OverdueAmount = 52.34m,
                    ContainBookingFees = true,
                    IsPaid = false
                });

                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var overduePayment = await client.GetOverduePayment();

            Assert.AreEqual(orderId, overduePayment.OrderId);
            Assert.AreEqual(12345, overduePayment.IBSOrderId);
            Assert.AreEqual("TransId", overduePayment.TransactionId);
            Assert.AreEqual(52.34m, overduePayment.OverdueAmount);
        }

        [Test]
        public async void when_settling_an_overdue_payment()
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

                var tokenizeResponse = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + string.Empty);
                var token = tokenizeResponse.CardOnFileToken;

                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);
                testAccount.DefaultCreditCard = creditCardId;

                context.RemoveAll<Booking.ReadModel.CreditCardDetails>();
                context.SaveChanges();

                context.Set<Booking.ReadModel.CreditCardDetails>().Add(new Booking.ReadModel.CreditCardDetails
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
        public async void when_deleting_a_tokenized_credit_card()
        {
            var client = GetPaymentClient();
            
            var tokenizeResponse = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            var response = await client.ForgetTokenizedCard(token);
            Assert.True(response.IsSuccessful, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate, TestCreditCards.AmericanExpress.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessful, response.Message);
        }
    }
}