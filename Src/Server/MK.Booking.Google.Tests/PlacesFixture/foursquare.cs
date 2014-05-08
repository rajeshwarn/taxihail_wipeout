using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;
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
       
    }
}