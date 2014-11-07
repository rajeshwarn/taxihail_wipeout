using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class NetworkRoamingFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
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
        public async void when_getting_the_local_company_market()
        {
            var sut = new NetworkRoamingServiceClient(BaseUrl, SessionId, new DummyPackageInfo());

            var market = await sut.GetLocalCompanyMarket(12.34, -77.43);
            Assert.AreEqual(string.Empty, market);
        }
    }
}
