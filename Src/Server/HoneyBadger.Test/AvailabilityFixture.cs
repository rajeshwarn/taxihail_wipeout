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
        public void GetAvailableVehicles_should_return_something()
        {
            var a = _sut.GetAvailableVehicles("BOS", 42.354045, -71.062289);
            Assert.IsNotEmpty(a);
        }
    }
}
