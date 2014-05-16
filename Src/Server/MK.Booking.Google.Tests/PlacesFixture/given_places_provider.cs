using System.Linq;
using apcurium.MK.Booking.MapDataProvider;
using NUnit.Framework;

namespace MK.Booking.Google.Tests.PlacesFixture
{
    public abstract class given_places_provider
    {
        protected const double Latitude = 45.5062;
        protected const double Longitude = -73.5726;
        protected IMapsApiClient Sut;

        [Test]
        public void when_searching_nearby_places()
        {
            var places = Sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);

            CollectionAssert.IsNotEmpty(places);
            Assert.NotNull(places.First().Name);
        }

        [Test]
        public void when_searching_nearby_places_it_does_not_include_neighborhoods()
        {
            var places = Sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 100);

            var neighborhoods = places.Where(x => x.Types.Contains("neighborhood"));

            Assert.IsEmpty(neighborhoods);
        }

        [Test]
        public void when_searching_nearby_places_with_name()
        {
            var places = Sut.SearchPlaces(Latitude, Longitude, "restaurants", "en", false, 800, "ca");

            CollectionAssert.IsNotEmpty(places);
            Assert.NotNull(places.First().Name);
        }
    }
}