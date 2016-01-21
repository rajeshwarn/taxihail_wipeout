using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Cryptography;
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
            _sut = new NetworkRoamingServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
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
            var market = await _sut.GetHashedCompanyMarket(99.99, -99.99);
            
            Assert.AreEqual(string.Empty, market);
        }

        [Test]
        public async void when_getting_the_company_market_settings()
        {
            var market = await _sut.GetCompanyMarketSettings(99.99, -99.99);

            Assert.AreEqual(string.Empty, market.HashedMarket);
            Assert.AreEqual(false, market.EnableDriverBonus);
            Assert.AreEqual(false, market.CancelOrderOnUnpair);
            Assert.AreEqual(false, market.OverrideEnableAppFareEstimates);
        }
    }
}
