﻿using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class NearbyPlacesFixture : BaseTest
    {
        private const double Latitude = 45.5062;
        private const double Longitude = -73.5726;

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
        public void when_searching_for_nearby_places()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId);
            var addresses = sut.GetNearbyPlaces(Latitude, Longitude);
            
            if (!addresses.Any())
            {
                Assert.Inconclusive("no places returned");
            }

            Assert.IsNotEmpty(addresses);
            CollectionAssert.AllItemsAreNotNull(addresses.Select(x => x.FriendlyName));
            CollectionAssert.AllItemsAreNotNull(addresses.Select(x => x.FullAddress));
        }

        [Test]
        public void when_searching_for_nearby_places_with_a_max_radius()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId);
            var greatRadius = sut.GetNearbyPlaces(Latitude, Longitude, radius: 100);
            var smallRadius = sut.GetNearbyPlaces(Latitude, Longitude, radius: 10);

            if (!greatRadius.Any() || !smallRadius.Any())
            {
                Assert.Inconclusive("no places returned");
            }

            Assert.IsNotEmpty(greatRadius);
            Assert.IsNotEmpty(smallRadius);
            Assert.Less(smallRadius.Count(), greatRadius.Count());
        }

        [Test]
        public void when_location_is_not_provided()
        {
            var sut = new NearbyPlacesClient(BaseUrl, SessionId);
            Assert.Throws<WebServiceException>(() => sut.GetNearbyPlaces(null, null), ErrorCode.NearbyPlaces_LocationRequired.ToString());
        }

        [Test]
        public void when_creating_an_order_with_a_nearby_place()
        {
            var orderId = Guid.NewGuid();
            var address = new NearbyPlacesClient(BaseUrl, SessionId).GetNearbyPlaces(Latitude, Longitude).FirstOrDefault();

            if (address == null)
            {
                Assert.Inconclusive("no places returned");
            }

            var sut = new OrderServiceClient(BaseUrl, SessionId);

            sut.CreateOrder(new CreateOrder
                                {
                                    Id = orderId,
                                    PickupAddress = address,
                                    Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 18, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" }
                                });
            var order = sut.GetOrder(orderId);

            Assert.NotNull(order);
            Assert.AreEqual(address.FullAddress, order.PickupAddress.FullAddress);
        }
    }
}
