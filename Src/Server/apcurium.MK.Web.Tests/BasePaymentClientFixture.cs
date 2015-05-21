using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
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
        public async void when_capturing_a_preauthorized_commit_a_credit_card_payment_but_ibs_failed()
        {
            _fakeIbs.Fail = true;

            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();

            var client = GetPaymentClient();
            var tokenizeResponse = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            using (var context = ContextFactory.Invoke())
            {
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

                context.SaveChanges();
            }

            var order = new CreateOrder
            {
                Id = orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new CreateOrder.RideEstimate
                {
                    Price = 65,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.CardOnFile.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "514-555-12129",
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1
                },
                Payment = new PaymentSettings
                {
                    CreditCardId = creditCardId,
                    PayWithCreditCard = true
                },
                ClientLanguageCode = "fr"
            };

            var orderServiceClient = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            try
            {
                await orderServiceClient.CreateOrder(order);
            }
            catch (WebServiceException ex)
            {
                Console.WriteLine(ex.ErrorMessage);
                throw;
            }

            // wait for ibs order id to be populated
            await Task.Delay(10000);

            const double amount = 31.50;
            const double meter = 21.25;
            const double tip = 10.25;

            // TODO: fix test
            //var response = await client.CommitPayment(token, amount, meter, tip, orderId);

            //CreditCardsCleanUp();

            //Assert.False(response.IsSuccessful);
            //Assert.True(response.Message.Contains("ibs failed"));
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

            // TODO: fix test
            //var authorization = await client.CommitPayment(token, amount, meter, tip, orderId);
            //Assert.True(authorization.IsSuccessful, authorization.Message);
        }

        [Test]
        public async void when_double_preauthorizing_and_capturing_then_error()
        {
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();

            var client = GetPaymentClient();

            var tokenizeResponse = await client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            var token = tokenizeResponse.CardOnFileToken;

            using (var context = ContextFactory.Invoke())
            {
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

                context.SaveChanges();
            }

            var order = new CreateOrder
            {
                Id = orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new CreateOrder.RideEstimate
                {
                    Price = 65,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.CardOnFile.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "514-555-12129",
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1
                },
                Payment = new PaymentSettings
                {
                    CreditCardId = creditCardId,
                    PayWithCreditCard = true
                },
                ClientLanguageCode = "fr"
            };

            var orderServiceClient = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            try
            {
                await orderServiceClient.CreateOrder(order);
            }
            catch (WebServiceException ex)
            {
                Console.WriteLine(ex.ErrorMessage);
                throw;
            }
            

            // wait for ibs order id to be populated
            await Task.Delay(10000);

            const double amount = 31.50;
            const double meter = 21.25;
            const double tip = 10.25;

            using (var context = ContextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().First(x => x.OrderId == orderId);
                payment.Amount = (decimal)amount;
                payment.Meter = (decimal)meter;
                payment.Tip = (decimal)tip;
                payment.IsCompleted = true;

                context.SaveChanges();
            }

            // TODO: fix test
            //var authorization = await client.CommitPayment(token, amount, meter, tip, orderId);

            //CreditCardsCleanUp();

            //Assert.False(authorization.IsSuccessful);
            //Assert.AreEqual("Order already paid or payment currently processing", authorization.Message);
        }

        private void CreditCardsCleanUp()
        {
            // Revert DB (delete test credit cards)
            using (var context = ContextFactory.Invoke())
            {
                var testAccount = context.Set<AccountDetail>().First(a => a.Id == TestAccount.Id);
                testAccount.DefaultCreditCard = null;

                var creditCardsModel = context.Set<Booking.ReadModel.CreditCardDetails>();

                foreach (var card in creditCardsModel)
                {
                    creditCardsModel.Remove(card);
                }
                context.SaveChanges();
            }
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