using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Google.Impl;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Google.Tests.PlacesFixture
{


    [TestFixture]
    public class given_a_gps_coordinate
    {
        private IMapsApiClient sut;
        

        [SetUp]
        public void Setup()
        {

            sut = new MapsApiClient(new TestConfigurationManager(), new Logger());

        }

        [Test]
        public void coordinate1_should_return_something()
        {
            var a = sut.GeocodeLocation(38.9040692, -77.0575374);            
            Assert.IsTrue(a.Results.Any());                
        }
    }
}

