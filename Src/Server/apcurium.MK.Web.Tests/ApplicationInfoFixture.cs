#region

using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

#endregion

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
        public void GetInfo()
        {
            var client = new ApplicationInfoServiceClient(BaseUrl, null, "Test");
            var appInfo = client.GetAppInfoAsync();


            Assert.AreEqual(GetType().Assembly.GetName().Version.ToString(), appInfo.Result.Version);
            Assert.AreEqual("Dev", appInfo.Result.SiteName);
        }
    }
}