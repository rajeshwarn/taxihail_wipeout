using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Web.Tests
{
    public class given_no_order : BaseTest
    {
        
        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();

        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        }

        [SetUp]
        public void SetupTest()
        {

        }

        [Test]
        public void create_order()
        {
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var pickupDate = DateTime.Now.AddHours(1);
            var requestDate = DateTime.Now.AddHours(1);
            var order = new CreateOrder
                            {
                                Id = Guid.NewGuid(),
                                AccountId = TestAccount.Id,
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
        public new void Setup()
        {
            base.Setup();
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var order = new CreateOrder
            {
                Id = _orderId,
                AccountId = TestAccount.Id,
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),

            };
            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };
            sut.CreateOrder(order);


        }

        [Test]
        public void ibs_order_was_created()
        {
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var order = sut.GetOrder( TestAccount.Id, _orderId);
            
            Assert.IsNotNull(order);
            Assert.IsNotNull(order.IBSOrderId);
        }


        [Test]
        public void GetOrderList()
        {

            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var orders = sut.GetOrders(TestAccount.Id);
            Assert.NotNull(orders);
        }

        [Test]
        public void GetOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var orders = sut.GetOrder(TestAccount.Id,_orderId);
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