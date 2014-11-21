using System.Linq;
using apcurium.MK.Booking.Common.Tests;
using NUnit.Framework;

namespace HoneyBadger.Test
{
    [TestFixture]
    public class AvailabilityFixture
    {
        [SetUp]
        public void Setup()
        {
            _sut = new HoneyBadgerServiceClient(new TestServerSettings());
        }

        private HoneyBadgerServiceClient _sut;

        [Test]
        public void when_getting_available_vehicles_inside_a_valide_zone()
        {
            var a = _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289);
            Assert.IsNotEmpty(a);
            Assert.LessOrEqual(a.Count(), 10);
        }

        [Test]
        public void when_getting_available_vehicles_outside_a_valide_zone()
        {
            var a = _sut.GetAvailableVehicles("BOS", 45.497765, -73.666280);
            Assert.IsEmpty(a);
        }
    }
}
