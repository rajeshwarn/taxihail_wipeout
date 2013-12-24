#region

using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Impl;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;

#endregion

namespace MK.Booking.Google.Tests.GeocodingFixture
{
    [TestFixture]
    public class given_a_gps_coordinate
    {
        [SetUp]
        public void Setup()
        {
            _sut = new MapsApiClient(new TestConfigurationManager(), new Logger());
        }

        private IMapsApiClient _sut;

        [Test]
        public void coordinate1_should_return_something()
        {
            var a = _sut.GeocodeLocation(38.9040692, -77.0575374);
            Assert.IsTrue(a.Results.Any());
        }
    }
}