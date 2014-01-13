using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class SearchLocationsFixture : BaseTest
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
        public async void when_name_then_places()
        {
            var sut = new SearchLocationsServiceClient(BaseUrl, SessionId, "Test");
            var addresses = await sut.Search("restaurant", 45.4983, -73.6586);

            if (!addresses.Any())
            {
                Assert.Inconclusive("no places returned");
            }

            Assert.True(addresses.Any());
            Assert.True(addresses.ElementAt(0).AddressType.Contains("place"));
        }

        [Test]
        public async void when_number_then_postal()
        {
            var sut = new SearchLocationsServiceClient(BaseUrl, SessionId, "Test");
            var addresses = await sut.Search("5661 chateaubriand", 45.5227967351675, -73.6242310144007);
            Assert.True(addresses.Any());
            Assert.True(addresses.ElementAt(0).AddressType.Contains("postal"));
        }
    }
}