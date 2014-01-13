using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class DirectionFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

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
        public async void BasicDirectionSearch()
        {
            var sut = new DirectionsServiceClient(BaseUrl, SessionId, "Test");
            var direction = await sut.GetDirectionDistance(45.5062, -73.5726, 45.5273, -73.6344);

            Assert.IsNotNull(direction);
            Assert.True(direction.Distance.HasValue);
            Assert.True(direction.Price.HasValue);
        }
    }
}