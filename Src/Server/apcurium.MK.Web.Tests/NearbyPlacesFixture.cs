﻿using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class NearbyPlacesFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        private const double Latitude = 45.531743;
        private const double Longitude = -73.622641;

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
        public async void when_creating_an_order_with_a_nearby_place()
        {
            var orderId = Guid.NewGuid();
            var addresses = await new NearbyPlacesClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).GetNearbyPlaces(Latitude, Longitude);
            var address = addresses.FirstOrDefault();

            if (address == null)
            {
                Assert.Inconclusive("no places returned");
            }

            var sut = new OrderServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            await sut.CreateOrder(new CreateOrderRequest
                {
                    Id = orderId,
                    PickupAddress = address,
                    Settings =
                        new BookingSettings
                            {
                                ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                                VehicleTypeId = 1,
                                ProviderId = Provider.ApcuriumIbsProviderId,
                                Phone = "5145551212",
                                Country = new CountryISOCode("CA"),
                                Passengers = 6,
                                NumberOfTaxi = 1,
                                Name = "Joe Smith"
                            },
                    Estimate = new RideEstimate
                        {
                            Distance = 3,
                            Price = 10
                        }
                });
            var order = await sut.GetOrder(orderId);

            Assert.NotNull(order);
            Assert.AreEqual(address.FullAddress, order.PickupAddress.FullAddress);
        }

        [Test]
        public void when_location_is_not_provided()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            Assert.Throws<WebServiceException>(async () => await sut.GetNearbyPlaces(null, null), ErrorCode.NearbyPlaces_LocationRequired.ToString());
        }

        [Test]
        public async void when_searching_for_nearby_places()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var addresses = await sut.GetNearbyPlaces(Latitude, Longitude);

            if (!addresses.Any())
            {
                Assert.Inconclusive("no places returned");
            }

            Assert.IsNotEmpty(addresses);
            CollectionAssert.AllItemsAreNotNull(addresses.Select(x => x.FriendlyName));
            CollectionAssert.AllItemsAreNotNull(addresses.Select(x => x.FullAddress));
        }

        [Test]
        public async void when_searching_for_nearby_places_with_a_max_radius()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var greatRadius = await sut.GetNearbyPlaces(Latitude, Longitude, 100);
            var smallRadius = await sut.GetNearbyPlaces(Latitude, Longitude, 10);

            if (!greatRadius.Any() || !smallRadius.Any())
            {
                Assert.Inconclusive("no places returned");
            }

            Assert.IsNotEmpty(greatRadius);
            Assert.IsNotEmpty(smallRadius);
            Assert.Less(smallRadius.Count(), greatRadius.Count());
        }
    }
}