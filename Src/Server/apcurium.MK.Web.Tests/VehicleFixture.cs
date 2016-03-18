using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Extensions;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class VehicleFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _sut = new VehicleServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
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
        public async Task get_available_vehicles()
        {
            var vehicles = await _sut.GetAvailableVehiclesAsync(45.420833, -75.69, 1);

            if (vehicles.Any())
            {
                Assert.Pass();
            }
            Assert.Inconclusive("Service returns no vehicles");
        }
    }
}