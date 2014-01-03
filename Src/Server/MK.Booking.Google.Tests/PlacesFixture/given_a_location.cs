#region

using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Impl;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;

#endregion

namespace MK.Booking.Google.Tests.PlacesFixture
{
    [TestFixture]
    public class given_a_location
    {
        [SetUp]
        public void Setup()
        {
            _sut = new MapsApiClient(new TestConfigurationManager(), new Logger());
        }

        private IMapsApiClient _sut;
        private const double Latitude = 45.5062;
        private const double Longitude = -73.5726;

        [Test]
        public void when_searching_nearby_places()
        {
            var places = _sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);

            CollectionAssert.IsNotEmpty(places);
            Assert.NotNull(places.First().Name);
        }

        [Test]
        public void when_searching_nearby_places_it_does_not_include_neighborhoods()
        {
            var places = _sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);

            var neighborhoods = places.Where(x => x.Types.Contains("neighborhood"));

            Assert.IsEmpty(neighborhoods);
        }

        [Test]
        public void when_searching_nearby_places_with_name()
        {
            var places = _sut.SearchPlaces(Latitude, Longitude, "restaurants", "en", false, 100, "ca");

            CollectionAssert.IsNotEmpty(places);
            Assert.NotNull(places.First().Name);
        }
    }
}