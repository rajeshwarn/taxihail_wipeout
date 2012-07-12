using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Web.Tests
{
    internal class AddressHistoryFixture : BaseTest
    {
        private Guid _knownAddressId = Guid.NewGuid();

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

        [Test]
        public void when_creating_an_order_with_a_new_pickup_address()
        {
            //Arrange
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var orderService = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            //Act
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                AccountId = TestAccount.Id,
                PickupAddress = TestAddresses.GetAddress1(),                
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),   
                
            };
            order.Settings = new Booking.Api.Contract.Resources.BookingSettings{ ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };             
            orderService.CreateOrder(order);

            //Assert
            var addresses = sut.GetHistoryAddresses(TestAccount.Id);
            Assert.AreEqual(1, addresses.Count());
        }
    }
}
