using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Google.Impl;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;

namespace apcurium.MK.Booking.Google.Tests.PlacesFixture
{
    [TestFixture]
    public class given_a_searchcriteria
    {
        private IAddresses sut;
        private const double Latitude = 45.5062;
        private const double Longitude = -73.5726;

        [SetUp]
        public void  Setup()
        {
            sut = new Addresses(new MapsApiClient(new TestConfigurationManager()), new TestConfigurationManager());
        }

        [Test]
        public void when_searching_for_a_uk_trainstation()
        {
            var places = sut.Search("Rye House Station", 45.466667,-73.75);
            
            CollectionAssert.IsNotEmpty(places);
            //Assert.NotNull(places.First().BuildingName);
        }

        //[Test]
        //public void when_searching_nearby_places_with_name()
        //{
        //    var places = sut.GetNearbyPlaces(Latitude, Longitude, "museum", "en", false, 100);

        //    CollectionAssert.IsNotEmpty(places);
        //    Assert.NotNull(places.First().Name);
        //}

        //[Test]
        //public void when_searching_nearby_places_it_does_not_include_neighborhoods()
        //{
        //    var places = sut.GetNearbyPlaces(Latitude, Longitude, null, "en", false, 100);

        //    var neighborhoods = places.Where(x => x.Types.Contains("neighborhood"));

        //    Assert.IsEmpty(neighborhoods);
            
        //}
    }
}
