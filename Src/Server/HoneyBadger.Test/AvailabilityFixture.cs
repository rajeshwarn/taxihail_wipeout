using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Test;
using apcurium.MK.Common.Diagnostic;
using CMTServices;

namespace HoneyBadger.Test
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
        public void when_getting_available_vehicles_inside_a_valid_zone()
        {
            var vehicles = _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289);
            Assert.IsNotEmpty(vehicles);
            Assert.LessOrEqual(vehicles.Count(), 10);
        }

        [Test]
        public void when_getting_available_vehicles_outside_a_valide_zone()
        {
            var vehicles = _sut.GetAvailableVehicles("BOS", 45.497765, -73.666280);
            Assert.IsEmpty(vehicles);
        }

        [Test]
        public void when_getting_the_status_of_a_vehicle()
        {
            var vehicles = _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289);
            Assert.IsNotEmpty(vehicles);

            var vehicleId = vehicles.First().Medallion;

            var vehicleStatus = _sut.GetVehicleStatus("BOS", new[] {vehicleId});
            Assert.IsNotEmpty(vehicleStatus);
        }
    }
}
