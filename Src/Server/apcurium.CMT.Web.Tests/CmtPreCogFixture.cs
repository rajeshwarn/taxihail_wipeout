using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;


namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class CmtPreCogFixture : CmtBaseTest
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
        public void when_sending_guide()
        {
            

            var sut = new CmtPreCogServiceClient(BaseUrl, null);
            var statusRequest = new PreCogRequest
                                 {
                                     LocLon = -73.97547,
                                     LocLat = 40.77690,
                                     LocTime = DateTime.Now,
                                     LocDesc = string.Empty,
                                     Init = true,
                                     Type = PreCogType.Status
                                 };
            sut.Send(statusRequest);
        }

    }
}
