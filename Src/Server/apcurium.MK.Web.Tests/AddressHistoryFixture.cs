﻿using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    internal class AddressHistoryFixture : BaseTest
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
        public async void when_creating_an_order_with_a_new_pickup_address()
        {
            //Arrange
            var newAccount = await CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());

            //Act
            var order = new CreateOrder
                {
                    Id = Guid.NewGuid(),
                    PickupAddress = TestAddresses.GetAddress1(),
                    PickupDate = DateTime.Now,
                    DropOffAddress = TestAddresses.GetAddress2(),
                    Estimate = new CreateOrder.RideEstimate
                        {
                            Distance = 3,
                            Price = 10
                        },
                    Settings = new BookingSettings
                        {
                            ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                            VehicleTypeId = 7,
                            ProviderId = Provider.MobileKnowledgeProviderId,
                            Phone = "514-555-1212",
                            Passengers = 6,
                            NumberOfTaxi = 1,
                            Name = "Joe Smith"
                        },
                   ClientLanguageCode = "en"
                };
            await orderService.CreateOrder(order);

            //Assert
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            var addresses = await sut.GetHistoryAddresses(newAccount.Id);
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
        public async void when_save_a_favorite_address_with_an_historic_address_existing()
        {
            //Arrange
            var newAccount = await CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupDate = DateTime.Now,
                PickupAddress =
                    new Address
                    {
                        FullAddress = "1234 rue Saint-Denis",
                        Apartment = "3939",
                        RingCode = "3131",
                        Latitude = 45.515065,
                        Longitude = -73.558064,
                    },
                DropOffAddress =
                    new Address {FullAddress = "Velvet auberge st gabriel", Latitude = 45.50643, Longitude = -73.554052},
                Settings = new BookingSettings
                {
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                    VehicleTypeId = 1,
                    ProviderId = Provider.MobileKnowledgeProviderId,
                    Phone = "514-555-1212",
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith"
                },
                Estimate = new CreateOrder.RideEstimate
                {
                    Distance = 3,
                    Price = 10
                },
                ClientLanguageCode = "en"
            };
            await orderService.CreateOrder(order);
            
            //Act
            var addressId = Guid.NewGuid();
            await sut.AddFavoriteAddress(new SaveAddress
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
            var addresses = await sut.GetHistoryAddresses(newAccount.Id);

            Assert.IsFalse(addresses.Any(x => x.Id.Equals(addressId)));
        }

        [Test]
        public async void when_removing_with_a_new_pickup_address()
        {
            //Arrange
            var newAccount = await CreateAndAuthenticateTestAccount();
            var orderService = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            
            var order = new CreateOrder
                {
                    Id = Guid.NewGuid(),
                    PickupAddress = TestAddresses.GetAddress1(),
                    PickupDate = DateTime.Now,
                    DropOffAddress = TestAddresses.GetAddress2(),
                    Estimate = new CreateOrder.RideEstimate
                        {
                            Distance = 3,
                            Price = 10
                        },
                    Settings = new BookingSettings
                        {
                            ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                            VehicleTypeId = 1,
                            ProviderId = Provider.MobileKnowledgeProviderId,
                            Phone = "514-555-1212",
                            Passengers = 6,
                            NumberOfTaxi = 1,
                            Name = "Joe Smith"
                        },
                    ClientLanguageCode = "en"
                };
            await orderService.CreateOrder(order);

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            var addresses = await sut.GetHistoryAddresses(newAccount.Id);

            //Act
            var addressId = addresses.First().Id;
            await sut.RemoveAddress(addressId);

            //Assert
            addresses = await sut.GetHistoryAddresses(newAccount.Id);
            Assert.AreEqual(false, addresses.Any(x => x.Id == addressId));
        }
    }
}