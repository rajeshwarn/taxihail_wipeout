﻿
#region

using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Common.Extensions;

#endregion

namespace MK.Booking.Google.Tests.GeocodingFixture
{
    [TestFixture]
    public class given_a_valid_address
    {
        private TestConfigurationManager _config = new TestConfigurationManager();
        [SetUp]
        public void Setup()
        {
            _sut = new GoogleApiClient(new TestConfigurationManager(), new Logger());
            
        }

        private IGeocoder _sut;

        [Test]
        public void address1_should_return_something()
        {
            _config.SetSetting("PriceFormat", "en-ca");
    
            var a = _sut.GeocodeAddress("5250 ferrier".Split(' ').JoinBy("+"), "en");
            Assert.IsTrue(a.Any());
        }

        [Test]
        public void yul1_should_return_something()
        {

            _config.SetSetting("PriceFormat", "en-ca");
            var a = _sut.GeocodeAddress("yul", "en");
            Assert.IsTrue(a.Any());
        }

    }
}