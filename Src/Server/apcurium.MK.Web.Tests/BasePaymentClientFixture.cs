using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Web.SelfHost;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.WebHost.Endpoints.Support;
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
            CreateAndAuthenticateTestAccount().Wait();
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
        [Ignore("Too much components tested, the notification to IBS is not working so the test failed. No easy way to disable the event handler of notification")]
        public async void when_authorized_a_credit_card_payment_and_resending_confirmation()
        {
            var orderServiceClient = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            var orderId = Guid.NewGuid();
            var order = new CreateOrder
            {
                Id = orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new CreateOrder.RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = 99,
                    VehicleTypeId = 1,
                    ProviderId = Provider.MobileKnowledgeProviderId,
                    Phone = "514-555-12129",
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1
                },
                ClientLanguageCode = "fr"
            };

            var details = await orderServiceClient.CreateOrder(order);

            var client = GetPaymentClient();

            var tokenClient = await client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            var token = tokenClient.CardOnFileToken;

            const double amount = 12.75;
            const double meter = 11.25;
            const double tip = 1.50;

            
            var response = await client.PreAuthorizeAndCommit(token, amount, meter, tip, orderId);
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
        public async void when_capturing_a_preauthorized_commit_a_credit_card_payment_but_ibs_failed()
        {
            _fakeIbs.Fail = true;
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

            
            var response = await client.PreAuthorizeAndCommit(token, amount, meter, tip, orderId);
            Assert.False(response.IsSuccessfull);
            Assert.AreEqual("ibs failed The transaction has been cancelled.", response.Message);
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
        [Ignore("Too much components tested, the notification to IBS is not working so the test failed. No easy way to disable the event handler of notification")]
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
                    VehicleNumber = "1001",
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
        public async void when_double_preauthorizing_and_capturing_then_error()
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
                    IBSOrderId = 1234,
                    VehicleNumber = "1001",
                    DriverInfos = new DriverInfos { DriverId = "10" },
                    PickupDate = DateTime.Now,
                    AccountId = TestAccount.Id
                });
                context.Set<OrderPaymentDetail>().Add(new OrderPaymentDetail
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = orderId
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
            Assert.False(authorization.IsSuccessfull);
            Assert.AreEqual("order already paid or payment currently processing", authorization.Message);
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

    public class FakeIbs : IIbsOrderService
    {
        public void ConfirmExternalPayment( Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type,
            string provider, string transactionId, string authorizationCode, string cardToken, int accountID, string name,
            string phone, string email, string os, string userAgent)
        {
            if (Fail)
            {
                throw new Exception("ibs failed");
            }
        }

        public void SendPaymentNotification(string message, string vehicleNumber, int ibsOrderId)
        {
            
        }

        public bool Fail { get; set; }
    }
}