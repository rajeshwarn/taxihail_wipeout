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

            var newAccount = GetNewAccount();
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, "password"));
            var orderService = new OrderServiceClient(BaseUrl, new AuthInfo(newAccount.Email, "password"));

            //Act
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                AccountId = newAccount.Id,
                PickupAddress = TestAddresses.GetAddress1(),                
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),                   
                
            };
            order.Settings = new Booking.Api.Contract.Resources.BookingSettings{ ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };             
            orderService.CreateOrder(order);

            //Assert
            var addresses = sut.GetHistoryAddresses(newAccount.Id);
            Assert.AreEqual(1, addresses.Count());
        }

        
        [Test]
        public void when_save_a_favorite_address_with_an_historic_address_existing()
        {
            //Setup
            var newAccount = GetNewAccount();
            
            var orderService = new OrderServiceClient(BaseUrl, new AuthInfo(newAccount.Email, "password"));

            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                AccountId = newAccount.Id,
                PickupDate = DateTime.Now,
                PickupAddress = new Booking.Api.Contract.Resources.Address { FullAddress = "1234 rue Saint-Denis", Apartment = "3939", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064,  },
                DropOffAddress = new Booking.Api.Contract.Resources.Address { FullAddress = "Velvet auberge st gabriel", Latitude = 45.50643, Longitude = -73.554052 },
                Settings = new Booking.Api.Contract.Resources.BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" }

            };
            orderService.CreateOrder(order);

            //Arrange
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, "password"));

            //Act
            var address = new SaveFavoriteAddress()
            {
                Id = Guid.NewGuid(),
                AccountId = newAccount.Id,
                FriendlyName = "La Boite à Jojo",
                FullAddress = "1234 rue Saint-Denis",
                Latitude = 45.515065,
                Longitude = -73.558064,
                Apartment = "3939",
                RingCode = "3131"
            };
            sut.AddFavoriteAddress(address);

            //Assert
            var addresses = sut.GetHistoryAddresses(newAccount.Id);
            Assert.AreEqual(0, addresses.Count());
        }
    }
}
