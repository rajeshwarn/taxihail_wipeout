using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Common.Diagnostic;
using MK.Booking.Google.Tests.PlacesFixture;
using MK.Booking.MapDataProvider.Foursquare;
using NUnit.Framework;

namespace MK.Booking.Google.Tests.Foursquare
{
    [TestFixture]
    public class foursquare : given_places_provider
    {
        [SetUp]
        public void Setup()
        {
            Sut = new FoursquareProvider(new TestConfigurationManager(), new Logger());
        }

        [Test]
        public void when_searching_nearby_places_it_does_not_include_others_categories()
        {
            //see here for categories https://developer.foursquare.com/categorytree
            var places = Sut.GetNearbyPlaces(Latitude, Longitude, "en", false, 800, "4d4b7105d754a06374d81259,4d4b7105d754a06376d81259"); //nightlife stop and restaurants

            var neighborhoods = places.Where(x => x.Types.Contains("neighborhood"));

            Assert.IsEmpty(neighborhoods);
        }

        [Test]
        public void when_getting_place_details()
        {
            var geoAddress = Sut.GetPlaceDetail("4ae4d07df964a520669e21e3");

            Assert.That(geoAddress.ZipCode, Is.EqualTo("H2X 1X7"));
        }
       
    }
}