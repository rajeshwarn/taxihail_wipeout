#region

using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class VehicleFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _sut = new VehicleServiceClient(BaseUrl, SessionId, new Logger(), "Test");
        }

        private VehicleServiceClient _sut;

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

        [Test]
        public void get_available_vehicles()
        {
            _sut.GetAvailableVehiclesAsync(45.420833, -75.69);

            Assert.Inconclusive("Service returns no vehicles");
        }
    }
}