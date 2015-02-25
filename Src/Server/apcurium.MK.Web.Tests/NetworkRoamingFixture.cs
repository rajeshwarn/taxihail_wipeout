using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class NetworkRoamingFixture : BaseTest
    {
        private NetworkRoamingServiceClient _sut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            _sut = new NetworkRoamingServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
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
        public async void when_getting_the_company_market()
        {
            var market = await _sut.GetCompanyMarket(12.34, -77.43);
            Assert.AreEqual(string.Empty, market);
        }
    }
}
