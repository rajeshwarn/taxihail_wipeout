﻿using System;
using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    internal class AddressHistoryFixture : BaseTest
    {
        private Guid _knownAddressId = Guid.NewGuid();

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
        public void when_creating_an_order_with_a_new_pickup_address()
        {
            //Arrange
            var newAccount = CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl, SessionId);

            //Act
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),                
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),                   
                
            };
            order.Settings = new Booking.Api.Contract.Resources.BookingSettings{ ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };             
            orderService.CreateOrder(order);

            //Assert
            var addresses = AccountService.GetHistoryAddresses(newAccount.Id);
            Assert.AreEqual(1, addresses.Count());
            var address = addresses.Single();

            Assert.AreEqual(order.PickupAddress.Apartment, address.Apartment);
            Assert.AreEqual(order.PickupAddress.RingCode, address.RingCode);
            Assert.AreEqual(order.PickupAddress.FullAddress, address.FullAddress);
            Assert.AreEqual(order.PickupAddress.BuildingName, address.BuildingName);
            Assert.AreEqual(order.PickupAddress.Latitude, address.Latitude);
            Assert.AreEqual(order.PickupAddress.Longitude, address.Longitude);
        }

        
        [Test]
        public void when_save_a_favorite_address_with_an_historic_address_existing()
        {
            //Arrange
            var newAccount = CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl,  SessionId);

            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupDate = DateTime.Now,
                PickupAddress = new Address { FullAddress = "1234 rue Saint-Denis", Apartment = "3939", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064,  },
                DropOffAddress = new Address { FullAddress = "Velvet auberge st gabriel", Latitude = 45.50643, Longitude = -73.554052 },
                Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" }

            };
            orderService.CreateOrder(order);


            //Act
            Guid addressId = Guid.NewGuid();
            AccountService.AddFavoriteAddress(new SaveAddress()
            {
                Id = addressId,
                Address = new Address
                {
                    FriendlyName = "La Boite à Jojo",
                    FullAddress = "1234 rue Saint-Denis",
                    Latitude = 45.515065,
                    Longitude = -73.558064,
                    Apartment = "3939",
                    RingCode = "3131"
                }
            });

            //Assert
            var addresses = AccountService.GetHistoryAddresses(newAccount.Id);

            Assert.IsFalse(addresses.Any(x => x.Id.Equals(addressId)));
            
        }


        [Test]
        public void when_removing_with_a_new_pickup_address()
        {
            //Arrange
            var newAccount = CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl, SessionId);

            
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),

            };
            order.Settings = new Booking.Api.Contract.Resources.BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };
            orderService.CreateOrder(order);

            var sut = AccountService;
            var addresses = sut.GetHistoryAddresses(newAccount.Id);

            //Act
            var addressId = addresses.First().Id;
            sut.RemoveAddress(addressId);

            //Assert
            addresses = sut.GetHistoryAddresses(newAccount.Id);
            Assert.AreEqual(false, addresses.Any(x => x.Id == addressId));
        }
    }
}
