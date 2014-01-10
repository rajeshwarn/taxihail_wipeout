using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class given_no_order : BaseTest
    {
        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();
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

        [Test]
        public async void create_order()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
                {
                    Id = Guid.NewGuid(),
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
                        }
                };

            var details = await sut.CreateOrder(order);

            Assert.NotNull(details);

            var orderDetails = await sut.GetOrder(details.OrderId);
            Assert.AreEqual(orderDetails.PickupAddress.FullAddress, order.PickupAddress.FullAddress);
            Assert.AreEqual(orderDetails.DropOffAddress.FullAddress, order.DropOffAddress.FullAddress);
            Assert.AreEqual(6, orderDetails.Settings.Passengers);
            Assert.AreEqual(1, orderDetails.Settings.LargeBags);
        }

        [Test]
        public void when_creating_order_without_passing_settings()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
                Estimate = new CreateOrder.RideEstimate
                {
                    Price = 10,
                    Distance = 3
                }
            };

            Assert.Throws<WebServiceException>(async () => await sut.CreateOrder(order), "CreateOrder_SettingsRequired");
        }
    }

    public class given_an_existing_order : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();

        [TestFixtureSetUp]
        public async new void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            var auth = await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(TestAccount.Email, TestAccountPassword);
            SessionId = auth.SessionId;

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
                {
                    Id = _orderId,
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
                            Phone = "514-555-1212",
                            Passengers = 6,
                            NumberOfTaxi = 1,
                            Name = "Joe Smith",
                            LargeBags = 1
                        }
                };
            await sut.CreateOrder(order);
        }

        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();
        }

        [Test]
        public async void ibs_order_was_created()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = await sut.GetOrder(_orderId);

            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IbsOrderId);
        }

        [Test]
        public async void can_not_get_order_another_account()
        {
            await CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            Assert.Throws<WebServiceException>(async () => await sut.GetOrder(_orderId));
        }

        [Test]
        public async void can_cancel_it()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            await sut.CancelOrder(_orderId);

            OrderStatusDetail status = null;
            for (var i = 0; i < 10; i++)
            {
                status = await sut.GetOrderStatus(_orderId);
                if (string.IsNullOrEmpty(status.IbsStatusId))
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
            Assert.AreEqual(VehicleStatuses.Common.CancelledDone, status.IbsStatusId);
        }

        [Test]
        public async void can_not_cancel_when_different_account()
        {
            await CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            Assert.Throws<WebServiceException>(async () => await sut.CancelOrder(_orderId));
        }

        [Test]
        public async void when_remove_it_should_not_be_in_history()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            await sut.RemoveFromHistory(_orderId);

            var orders = await sut.GetOrders();
            Assert.AreEqual(false, orders.Any(x => x.Id == _orderId));
        }

        [Test]
        public async void when_order_rated_ratings_should_not_be_null()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            var orderRatingsRequest = new OrderRatingsRequest
            {
                OrderId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            };

            await sut.RateOrder(orderRatingsRequest);

            var orderRatingDetails = await sut.GetOrderRatings(_orderId);

            Assert.NotNull(orderRatingDetails);
            Assert.That(orderRatingDetails.Note, Is.EqualTo(orderRatingsRequest.Note));
            Assert.That(orderRatingDetails.RatingScores.Count, Is.EqualTo(orderRatingsRequest.RatingScores.Count));
        }

        [Test]
        public async void GetOrderList()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            var orders = await sut.GetOrders();
            Assert.NotNull(orders);
        }

        [Test]
        public async void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            var orders = await sut.GetOrder(_orderId);
            Assert.NotNull(orders);

            //TODO: Fix test

            Assert.AreEqual(TestAddresses.GetAddress1().Apartment, orders.PickupAddress.Apartment);
            Assert.AreEqual(TestAddresses.GetAddress1().FullAddress, orders.PickupAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress1().RingCode, orders.PickupAddress.RingCode);
            Assert.AreEqual(TestAddresses.GetAddress1().Latitude, orders.PickupAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress1().Longitude, orders.PickupAddress.Longitude);
            Assert.AreEqual(TestAddresses.GetAddress2().FullAddress, orders.DropOffAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress2().Latitude, orders.DropOffAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress2().Longitude, orders.DropOffAddress.Longitude);
            Assert.AreNotEqual(OrderStatus.Completed, orders.Status);
            Assert.IsNull(orders.Fare);
            Assert.IsNull(orders.Toll);
            Assert.IsNull(orders.Tip);
        }
    }
}