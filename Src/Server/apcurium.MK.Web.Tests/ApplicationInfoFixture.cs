using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Web.Tests
{
     [TestFixture]
    public class ApplicationInfoFixture : BaseTest
    {
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

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }


         [Test]
         public void GetInfo()
         {
             var client = new ApplicationInfoServiceClient(BaseUrl, null, "Test");
             var appInfo = client.GetAppInfoAsync();


             Assert.AreEqual(this.GetType().Assembly.GetName().Version.ToString(), appInfo.Result.Version);
             Assert.AreEqual("Dev", appInfo.Result.SiteName);
             

         }
    }
}
