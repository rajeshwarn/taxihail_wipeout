using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ApplicationInfoFixture : BaseTest
    {
        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();
        }

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async void GetInfo()
        {
            var client = new ApplicationInfoServiceClient(BaseUrl, null, "Test");
            var appInfo = await client.GetAppInfoAsync();
            
            Assert.AreEqual(GetType().Assembly.GetName().Version.ToString(), appInfo.Version);
            Assert.AreEqual("Dev", appInfo.SiteName);
        }
    }
}