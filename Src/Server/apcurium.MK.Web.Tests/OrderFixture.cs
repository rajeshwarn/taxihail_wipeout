#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

#endregion

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class given_no_order : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
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
        public void create_order()
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

            order.Settings = new BookingSettings
            {
                ChargeTypeId = 99,
                VehicleTypeId = 1,
                ProviderId = Provider.MobileKnowledgeProviderId,
                Phone = "514-555-12129",
                Passengers = 6,
                NumberOfTaxi = 1,
                Name = "Joe Smith",
                LargeBags = 1
            };

            var details = sut.CreateOrder(order);

            Assert.NotNull(details);

            var orderDetails = sut.GetOrder(details.OrderId);
            Assert.AreEqual(orderDetails.PickupAddress.FullAddress, order.PickupAddress.FullAddress);
            Assert.AreEqual(orderDetails.DropOffAddress.FullAddress, order.DropOffAddress.FullAddress);
            Assert.AreEqual(6, orderDetails.Settings.Passengers);
            Assert.AreEqual(1, orderDetails.Settings.LargeBags);
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException",
            ExpectedMessage = "CreateOrder_SettingsRequired")]
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
            sut.CreateOrder(order);
        }
    }

    public class given_an_existing_order : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();

        [TestFixtureSetUp]
        public new void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            var auth = new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(TestAccount.Email,
                TestAccountPassword);
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
                }
            };
            order.Settings = new BookingSettings
            {
                ChargeTypeId = 99,
                VehicleTypeId = 1,
                ProviderId = Provider.MobileKnowledgeProviderId,
                Phone = "514-555-1212",
                Passengers = 6,
                NumberOfTaxi = 1,
                Name = "Joe Smith",
                LargeBags = 1
            };
            sut.CreateOrder(order);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void ibs_order_was_created()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = sut.GetOrder(_orderId);

            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IbsOrderId);
        }

        [Test]
        public void can_not_get_order_another_account()
        {
            CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            Assert.Throws<WebServiceException>(() => sut.GetOrder(_orderId));
        }

        [Test]
        public void can_cancel_it()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            sut.CancelOrder(_orderId);

            OrderStatusDetail status = null;
            for (var i = 0; i < 10; i++)
            {
                status = sut.GetOrderStatus(_orderId);
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
        public void can_not_cancel_when_different_account()
        {
            CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            Assert.Throws<WebServiceException>(() => sut.CancelOrder(_orderId));
        }

        [Test]
        public void when_remove_it_should_not_be_in_history()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            sut.RemoveFromHistory(_orderId);

            var orders = sut.GetOrders();
            Assert.AreEqual(false, orders.Any(x => x.Id == _orderId));
        }

        [Test]
        public void when_order_rated_ratings_should_not_be_null()
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

            sut.RateOrder(orderRatingsRequest);

            var orderRatingDetails = sut.GetOrderRatings(_orderId);

            Assert.NotNull(orderRatingDetails);
            Assert.That(orderRatingDetails.Note, Is.EqualTo(orderRatingsRequest.Note));
            Assert.That(orderRatingDetails.RatingScores.Count, Is.EqualTo(orderRatingsRequest.RatingScores.Count));
        }

        [Test]
        public void GetOrderList()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            var orders = sut.GetOrders();
            Assert.NotNull(orders);
        }

        [Test]
        public void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            var orders = sut.GetOrder(_orderId);
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