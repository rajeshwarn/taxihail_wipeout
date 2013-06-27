using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    public class given_no_order : BaseTest
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

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void create_order()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };
            
            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1 , ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var details = sut.CreateOrder(order);

            Assert.NotNull(details);

            var orderDetails = sut.GetOrder(details.OrderId);
            Assert.AreEqual(orderDetails.PickupAddress.FullAddress, order.PickupAddress.FullAddress);
            Assert.AreEqual(orderDetails.DropOffAddress.FullAddress, order.DropOffAddress.FullAddress);
            Assert.AreEqual(6, orderDetails.Settings.Passengers);
        }
        
        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateOrder_SettingsRequired")]
        public void when_creating_order_without_passing_settings()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };
            sut.CreateOrder(order);
        }

    }
    public class give_an_existing_order : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();

        [TestFixtureSetUp]
        public new void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            var auth = new AuthServiceClient(BaseUrl, SessionId).Authenticate(TestAccount.Email, TestAccountPassword);
            SessionId = auth.SessionId;

            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = new CreateOrder
            {
                Id = _orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),

            };
            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };
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
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = sut.GetOrder(_orderId);
            
            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IBSOrderId);
        }

        [Test]
        public void order_was_payed()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            sut.FinailizePayment(90,"1234",1234,_orderId,1234);

        }

        [Test]
        public void can_not_get_order_another_account()
        {
            CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId);
            Assert.Throws<WebServiceException>(() => sut.GetOrder(_orderId));
        }

        [Test]
        public void can_cancel_it()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            sut.CancelOrder(_orderId);

            var status = sut.GetOrderStatus(_orderId);

            Assert.AreEqual(OrderStatus.Canceled, status.Status);
            Assert.AreEqual("wosCANCELLED_DONE", status.IBSStatusId);
        }

        [Test]
        public void can_not_cancel_when_different_account()
        {
            CreateAndAuthenticateTestAccount();
              
            var sut = new OrderServiceClient(BaseUrl, SessionId);

            Assert.Throws<WebServiceException>(() => sut.CancelOrder(_orderId));
        }

        [Test]
        public void when_remove_it_should_not_be_in_history()
        {

            var sut = new OrderServiceClient(BaseUrl, SessionId);

            sut.RemoveFromHistory(_orderId);

            var orders = sut.GetOrders();
            Assert.AreEqual(false, orders.Any(x => x.Id == _orderId));
        }

        [Test]
        public void when_order_rated_ratings_should_not_be_null()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);

            var orderRatingsRequest = new OrderRatingsRequest
                {
                    OrderId =_orderId,
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

            var sut = new OrderServiceClient(BaseUrl, SessionId);

            var orders = sut.GetOrders();
            Assert.NotNull(orders);
        }

        [Test]
        public void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId);

            var orders = sut.GetOrder(_orderId);
            Assert.NotNull(orders);
            
            //TODO: Fix test

            Assert.AreEqual(TestAddresses.GetAddress1().Apartment , orders.PickupAddress.Apartment );
            Assert.AreEqual(TestAddresses.GetAddress1().FullAddress, orders.PickupAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress1().RingCode, orders.PickupAddress.RingCode);
            Assert.AreEqual(TestAddresses.GetAddress1().Latitude, orders.PickupAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress1().Longitude, orders.PickupAddress.Longitude);
            Assert.AreEqual(TestAddresses.GetAddress2().FullAddress, orders.DropOffAddress.FullAddress);
            Assert.AreEqual(TestAddresses.GetAddress2().Latitude, orders.DropOffAddress.Latitude);
            Assert.AreEqual(TestAddresses.GetAddress2().Longitude, orders.DropOffAddress.Longitude);
            Assert.AreEqual(false, orders.IsCompleted);
            Assert.IsNull(orders.Fare);
            Assert.IsNull(orders.Toll);
            Assert.IsNull(orders.Tip);

        }
    }
}