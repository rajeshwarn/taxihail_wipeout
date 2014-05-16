#region

using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;

#endregion

namespace MK.Booking.Google.Tests.PlacesFixture
{
    [TestFixture]
    public class googlemaps : given_places_provider
    {
        [SetUp]
        public void Setup()
        {
            Sut = new MapsApiClient(new TestConfigurationManager(), new Logger());
        }

    }
}