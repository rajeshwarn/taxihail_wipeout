
#region

using System.Linq;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Booking.Test;
using apcurium.MK.Common.Extensions;

#endregion

namespace MK.Booking.Google.Tests.GeocodingFixture
{
    [TestFixture]
    public class given_a_valid_address
    {
        private TestServerSettings _config = new TestServerSettings();
        [SetUp]
        public void Setup()
        {
            _sut = new GoogleApiClient(new TestServerSettings(), new Logger(), null);
            
        }

        private IGeocoder _sut;

        [Test]
        public void address1_should_return_something()
        {
            _config.SetSetting("PriceFormat", "en-ca");
    
            var a = _sut.GeocodeAddress("5250 ferrier montreal".Split(' ').JoinBy("+"), "en", 45.5323177, -73.629156, 45000);
            Assert.IsTrue(a.Any());
        }

        [Test]
        public void yul1_should_return_something()
        {

            _config.SetSetting("PriceFormat", "en-ca");
            var a = _sut.GeocodeAddress("yul", "en", 45.5323177, -73.629156, 45000);
            Assert.IsTrue(a.Any());
        }

    }
}