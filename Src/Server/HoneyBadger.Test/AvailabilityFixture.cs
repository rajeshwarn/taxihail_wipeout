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
            _sut = new HoneyBadgerServiceClient();
        }

        private HoneyBadgerServiceClient _sut;

        [Test]
        public void GetAvailableVehicles_should_return_something()
        {
            var a = _sut.GetAvailableVehicles("BOS", "1020301");
            Assert.IsNotEmpty(a);
        }
    }
}
