using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Test;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;

namespace CMTServices.Test
{
    [TestFixture]
    public class AvailabilityFixture
    {
        [SetUp]
        public void Setup()
        {
            _sut = new HoneyBadgerServiceClient(new TestServerSettings(), new Logger());
        }

        private HoneyBadgerServiceClient _sut;

        [Test]
        public async Task when_getting_available_vehicles_inside_a_valid_zone()
        {
            var vehicles = (await _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289)).ToArray();
            Assert.IsNotEmpty(vehicles);
            Assert.LessOrEqual(vehicles.Count(), 10);
        }

        [Test]
        public async Task when_getting_available_vehicles_outside_a_valide_zone()
        {
            var vehicles = await _sut.GetAvailableVehicles("BOS", 45.497765, -73.666280);
            Assert.IsEmpty(vehicles);
        }

        [Test]
        public async Task when_getting_the_status_of_a_vehicle()
        {
            var vehicles = (await _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289)).ToArray();
            Assert.IsNotEmpty(vehicles);

            var vehicleId = vehicles.First().Medallion;

            var vehicleStatus = await _sut.GetVehicleStatus("BOS", new[] {vehicleId});
            Assert.IsNotEmpty(vehicleStatus);
        }
    }
}
