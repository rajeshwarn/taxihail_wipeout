using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ApplicationInfoFixture : BaseTest
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
        public async void GetInfo()
        {
            var client = new ApplicationInfoServiceClient(BaseUrl, null, new DummyPackageInfo());
            var appInfo = await client.GetAppInfoAsync();
            
            Assert.AreEqual(GetType().Assembly.GetName().Version.ToString(), appInfo.Version);
            Assert.AreEqual("Dev", appInfo.SiteName);
        }
    }
}