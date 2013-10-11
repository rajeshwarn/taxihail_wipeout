using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class VehicleFixture : BaseTest
    {
        private VehicleServiceClient sut;

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            sut = new VehicleServiceClient(BaseUrl, SessionId, new Logger());
        }

        [Test]
        public void get_available_vehicles()
        {
            var result = sut.GetAvailableVehicles(45.420833, -75.69);

            Assert.Inconclusive("Service returns no vehicles");
        }
    }
}
