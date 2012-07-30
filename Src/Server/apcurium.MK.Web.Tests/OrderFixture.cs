using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Resources;

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
            var sut = new OrderServiceClient(BaseUrl);
            var pickupDate = DateTime.Now.AddHours(1);
            var requestDate = DateTime.Now.AddHours(1);
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var id = sut.CreateOrder(order);
            
            
            Assert.NotNull(id);
        }



    }
    public class give_an_existing_order : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();

        [TestFixtureSetUp]
        public new void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            new AuthServiceClient(BaseUrl).Authenticate(TestAccount.Email, TestAccountPassword);    

            var sut = new OrderServiceClient(BaseUrl);
            var order = new CreateOrder
            {
                Id = _orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),

            };
            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };
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
            var sut = new OrderServiceClient(BaseUrl);
            var order = sut.GetOrder(_orderId);
            
            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IBSOrderId);
        }

        [Test]
        public void can_not_get_order_another_account()
        {
            CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl);
            Assert.Throws<WebServiceException>(() => sut.GetOrder(_orderId));
        }

        [Test]
        public void can_cancel_it()
        {
            var sut = new OrderServiceClient(BaseUrl);
            sut.CancelOrder(_orderId);

            var status = sut.GetOrderStatus(_orderId);

            Assert.AreEqual("Cancelled", status.IBSStatusDescription);
        }

        [Test]
        public void can_not_cancel_when_different_account()
        {
            CreateAndAuthenticateTestAccount();
              
            var sut = new OrderServiceClient(BaseUrl);

            Assert.Throws<WebServiceException>(() => sut.CancelOrder(_orderId));
        }


        [Test]
        public void GetOrderList()
        {

            var sut = new OrderServiceClient(BaseUrl);

            var orders = sut.GetOrders();
            Assert.NotNull(orders);
        }

        [Test]
        public void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl);

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

        }
    }
}