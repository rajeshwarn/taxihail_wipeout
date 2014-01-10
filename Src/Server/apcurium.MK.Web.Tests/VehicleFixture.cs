using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class VehicleFixture : BaseTest
    {
        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();

            _sut = new VehicleServiceClient(BaseUrl, SessionId, "Test");
        }

        private VehicleServiceClient _sut;

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async void get_available_vehicles()
        {
            await _sut.GetAvailableVehiclesAsync(45.420833, -75.69);

            Assert.Inconclusive("Service returns no vehicles");
        }
    }
}