#region

using System.Linq;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Booking.Test;

#endregion

namespace MK.Booking.Google.Tests.GeocodingFixture
{
    [TestFixture]
    public class given_a_gps_coordinate
    {
        [SetUp]
        public void Setup()
        {
            _sut = new GoogleApiClient(new TestServerSettings(), new Logger(), null);
            
        }

        private IGeocoder _sut;

        [Test]
        public void coordinate1_should_return_something()
        {
            var a = _sut.GeocodeLocation(38.9040692, -77.0575374, "en");
            Assert.IsTrue(a.Any());
        }
    }
}