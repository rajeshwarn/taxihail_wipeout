using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class OrderStatusFixture : BaseTest
    {

        private Guid _orderId;
       
        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            _orderId = Guid.NewGuid();
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var order = new CreateOrder
            {
                Id = _orderId,
                AccountId = TestAccount.Id,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                PickupDate = DateTime.Now,
                Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" }
            };

            sut.CreateOrder(order);
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
        public void create_and_get_a_valid_order()
        {
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var data = sut.GetOrderStatus( TestAccount.Id, _orderId);
            //Assert.AreEqual(OrderStatus, data.Status);
        }
    }
}