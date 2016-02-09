using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using CustomerPortal.Client;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class given_no_order : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
        }

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

        [Test]
        public async void create_order()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = new CreateOrderRequest
                {
                    Id = Guid.NewGuid(),
                    PickupAddress = TestAddresses.GetAddress1(),
                    DropOffAddress = TestAddresses.GetAddress2(),
                    Estimate = new RideEstimate
                        {
                            Price = 10,
                            Distance = 3
                        },
                    Settings = new BookingSettings
                        {
                            ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                            VehicleTypeId = 1,
                            ProviderId = Provider.ApcuriumIbsProviderId,
                            Phone = "5145551212",
                            Country = new CountryISOCode("CA"),
                            Passengers = 6,
                            NumberOfTaxi = 1,
                            Name = "Joe Smith",
                            LargeBags = 1,
                            AccountNumber = "123",
                            CustomerNumber = "0"
                        },
                    ClientLanguageCode = SupportedLanguages.fr.ToString()
                };

            var details = await sut.CreateOrder(order);

            Assert.NotNull(details);

            var orderDetails = await sut.GetOrder(details.OrderId);
            Assert.AreEqual(orderDetails.PickupAddress.FullAddress, order.PickupAddress.FullAddress);
            Assert.AreEqual(orderDetails.DropOffAddress.FullAddress, order.DropOffAddress.FullAddress);
            Assert.AreEqual(6, orderDetails.Settings.Passengers);
            Assert.AreEqual(1, orderDetails.Settings.LargeBags);
            Assert.AreEqual("123", orderDetails.Settings.AccountNumber);
            Assert.AreEqual("0", orderDetails.Settings.CustomerNumber);
        }

        [Test]
        public void create_order_with_charge_account_with_card_on_file_payment_from_web_app()
        {
            var accountChargeSut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
            var accountChargeName = "NAME" + new Random(DateTime.Now.Millisecond).Next(0, 5236985);
            var accountChargeNumber = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985);
            var accountCustomerNumber = "CUSTOMER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985);

            accountChargeSut.CreateAccountCharge(new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = accountChargeName,
                AccountNumber = accountChargeNumber,
                UseCardOnFileForPayment = true,
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Question = "Question?",
                        Answer = "Answer"
                    }
                }
            });

            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo { UserAgent = "FireFox" }, null, null);
            var order = new CreateOrderRequest
            {
                Id = Guid.NewGuid(),
                FromWebApp = true,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.Account.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "5145551212",
                    Country = new CountryISOCode("CA"),
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1,
                    AccountNumber = accountChargeNumber,
                    CustomerNumber = accountCustomerNumber
                },
                Payment = new PaymentSettings
                {
                    CreditCardId = Guid.NewGuid(),
                    TipPercent = 15
                },
                QuestionsAndAnswers = new[]
                {
                    new AccountChargeQuestion
                    {
                        Answer = "Answer"
                    }
                },
                ClientLanguageCode = SupportedLanguages.fr.ToString()
            };

            var ex = Assert.Throws<WebServiceException>(async () => await sut.CreateOrder(order));
            Assert.AreEqual("Ce compte n'est pas supporté par la page web", ex.ErrorMessage);
        }

        [Test]
        public async void create_order_with_user_location()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = new CreateOrderRequest
            {
                Id = Guid.NewGuid(),
                UserLatitude = 46.50643,
                UserLongitude = -74.554052,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "5145551212",
                    Country = new CountryISOCode("CA"),
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1
                },
                ClientLanguageCode = SupportedLanguages.en.ToString()
            };

            await sut.CreateOrder(order);

            using (var context = new BookingDbContext(ConfigurationManager.ConnectionStrings["MKWebDev"].ConnectionString))
            {
                var infoGPS = context.Find<OrderUserGpsDetail>(order.Id);
                Assert.AreEqual(order.UserLatitude, infoGPS.UserLatitude);
                Assert.AreEqual(order.UserLongitude, infoGPS.UserLongitude);
            }
        }

        [Test]
        public void when_creating_order_without_passing_settings()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = new CreateOrderRequest
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    Phone = "5145551212",
                    Country = new CountryISOCode("CA")
                },
                ClientLanguageCode = SupportedLanguages.fr.ToString()
            };

            var ex = Assert.Throws<WebServiceException>(async () => await sut.CreateOrder(order));
            Assert.AreEqual("CreateOrder_SettingsRequired", ex.ErrorMessage);
        }

        [Test]
        public void when_creating_order_with_promotion_but_not_using_card_on_file()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = new CreateOrderRequest
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "5145551212",
                    Country = new CountryISOCode("CA"),
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1,
                    AccountNumber = "123",
                    CustomerNumber = "0"
                },
                ClientLanguageCode = SupportedLanguages.fr.ToString(),
                PromoCode = "123"
            };

            var ex = Assert.Throws<WebServiceException>(async () => await sut.CreateOrder(order));
            Assert.AreEqual("Vous devez sélectionner le Paiement In App pour utiliser une promotion.", ex.ErrorMessage);
        }
    }

    public class given_an_existing_order : BaseTest
    {
        private Guid _orderId;
        private Type _taxiHailNetworkServiceImplementation;
        private FakeTaxiHailNetworkServiceClient _taxiHailNetworkService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            //replace it with a fake
            _taxiHailNetworkService = new FakeTaxiHailNetworkServiceClient("x2s42");
            UnityServiceLocator.Instance.RegisterInstance<ITaxiHailNetworkServiceClient>(_taxiHailNetworkService);
        }

        [TestFixtureSetUp]
        public new void TestFixtureSetup()
        {
            base.TestFixtureSetup();
            var container = UnityServiceLocator.Instance;
            _taxiHailNetworkServiceImplementation = container.Registrations
                                            .FirstOrDefault(x => x.RegisteredType == typeof(ITaxiHailNetworkServiceClient))
                                            .MappedToType;
            
            _orderId = Guid.NewGuid();

            var authTask = new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(TestAccount.Email, TestAccountPassword);
            authTask.Wait();
            var auth = authTask.Result;
            SessionId = auth.SessionId;

            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = new CreateOrderRequest
            {
                Id = _orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new RideEstimate
                {
                    Price = 10,
                    Distance = 3
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.ApcuriumIbsProviderId,
                    Phone = "5145551212",
                    Country = new CountryISOCode("CA"),
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith",
                    LargeBags = 1
                },
                ClientLanguageCode = SupportedLanguages.fr.ToString()
            };
            sut.CreateOrder(order).Wait();

            // Wait for IBS order Id to be assigned
            Thread.Sleep(10000);
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
            UnityServiceLocator.Instance.RegisterType(typeof(ITaxiHailNetworkServiceClient), _taxiHailNetworkServiceImplementation);
        }

        [Test]
        public async void try_to_switch_order_to_next_dispatch_company_when_not_timedout()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = await sut.GetOrderStatus(_orderId);

            var orderStatus = await sut.SwitchOrderToNextDispatchCompany(new SwitchOrderToNextDispatchCompanyRequest
                {
                    OrderId = _orderId,
                    NextDispatchCompanyKey = "x2s42",
                    NextDispatchCompanyName = "Vector Industries"
                });

            Assert.NotNull(orderStatus);
            Assert.AreEqual(_orderId, orderStatus.OrderId);
            Assert.AreEqual(order.IBSOrderId, orderStatus.IBSOrderId);
            Assert.AreEqual(order.Status, orderStatus.Status);
            Assert.IsNull(orderStatus.NextDispatchCompanyKey);
            Assert.IsNull(orderStatus.NextDispatchCompanyName);
        }

        [Test]
        public async void order_dispatch_company_switch_ignored()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            await sut.IgnoreDispatchCompanySwitch(_orderId);

            var status = await sut.GetOrderStatus(_orderId);

            Assert.NotNull(status);
            Assert.AreEqual(true, status.IgnoreDispatchCompanySwitch);
            Assert.AreEqual(OrderStatus.Created, status.Status);
            Assert.IsNull(status.NextDispatchCompanyKey);
            Assert.IsNull(status.NextDispatchCompanyName);
        }

        [Test]
        public async void ibs_order_was_created()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var order = await sut.GetOrder(_orderId);

            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IBSOrderId);
        }

        [Test]
        public async void can_not_get_order_another_account()
        {
            await CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var ex = Assert.Throws<WebServiceException>(async () => await sut.GetOrder(_orderId));
            Assert.AreEqual("Can't access another account's order", ex.Message);
        }

        [Test]
        public async void can_cancel_it()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            await sut.CancelOrder(_orderId);

            OrderStatusDetail status = null;
            for (var i = 0; i < 10; i++)
            {
                status = await sut.GetOrderStatus(_orderId);
                if (string.IsNullOrEmpty(status.IBSStatusId))
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }

            Assert.NotNull(status);
            Assert.AreEqual(OrderStatus.Canceled, status.Status);
            Assert.AreEqual(VehicleStatuses.Common.CancelledDone, status.IBSStatusId);
        }

        [Test]
        public async void can_not_cancel_when_different_account()
        {
            await CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var ex = Assert.Throws<WebServiceException>(async () => await sut.CancelOrder(_orderId));
            Assert.AreEqual("Can't cancel another account's order", ex.Message);
        }

        [Test]
        public async void when_remove_it_should_not_be_in_history()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            await sut.RemoveFromHistory(_orderId);

            var orders = await sut.GetOrders();
            Assert.AreEqual(false, orders.Any(x => x.Id == _orderId));
        }

        [Test]
        public async void when_order_rated_ratings_should_not_be_null()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var orderRatingsRequest = new OrderRatingsRequest
            {
                OrderId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            };

            await sut.RateOrder(orderRatingsRequest);

            var orderRatingDetails = await sut.GetOrderRatings(_orderId);

            Assert.NotNull(orderRatingDetails);
            Assert.That(orderRatingDetails.Note, Is.EqualTo(orderRatingsRequest.Note));
            Assert.That(orderRatingDetails.RatingScores.Count, Is.EqualTo(2));
        }

        [Test]
        public async void GetOrderList()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var orders = await sut.GetOrders();
            Assert.NotNull(orders);
        }

        [Test]
        public async void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var order = await sut.GetOrder(_orderId);
            Assert.NotNull(order);

            Assert.AreEqual(TestAddresses.GetAddress1().Apartment, order.PickupAddress.Apartment);
            Assert.AreEqual(TestAddresses.GetAddress1().FullAddress, order.PickupAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress1().RingCode, order.PickupAddress.RingCode);
            Assert.AreEqual(TestAddresses.GetAddress1().Latitude, order.PickupAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress1().Longitude, order.PickupAddress.Longitude);
            Assert.AreEqual(TestAddresses.GetAddress2().FullAddress, order.DropOffAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress2().Latitude, order.DropOffAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress2().Longitude, order.DropOffAddress.Longitude);
            Assert.AreNotEqual(OrderStatus.Completed, order.Status);
        }
    }
}