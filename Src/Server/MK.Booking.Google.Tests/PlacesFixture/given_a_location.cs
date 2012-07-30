using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Google.Impl;

namespace apcurium.MK.Booking.Google.Tests.PlacesFixture
{
    [TestFixture]
    public class given_a_location
    {
        private IPlacesClient sut;
        private const double Latitude = 45.5062;
        private const double Longitude = -73.5726;

        [SetUp]
        public void  Setup()
        {
            sut = new PlacesClient();
        }

        [Test]
        public void when_searching_nearby_places()
        {
            var places = sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);
            
            CollectionAssert.IsNotEmpty(places);
            Assert.NotNull(places.First().Name);
        }

        [Test]
        public void when_searching_nearby_places_it_does_not_include_neighborhoods()
        {
            var places = sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);

            var neighborhoods = places.Where(x => x.Types.Contains("neighborhood"));

            Assert.IsEmpty(neighborhoods);
            
        }
    }
}
