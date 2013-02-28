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
            Authenticate();
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
        public void when_sending_status()
        {
            

            var sut = new CmtPreCogServiceClient(BaseUrl, Credentials);
            var statusRequest = new PreCogRequest
                                 {
                                     LocLon = -74.004593,
                                     LocLat = 40.740676,
                                     LocTime = DateTime.Now,
                                     LocDesc = "Apcurium",
                                     Init = true,
                                     Type = PreCogType.Status
                                 };
            sut.Send(statusRequest, false);
        }

        [Test]
        public void when_sending_brodcast()
        {

            var sut = new CmtPreCogServiceClient(BaseUrl, Credentials);
            var statusRequest = new PreCogRequest
            {
                LocLon = -74.004593,
                LocLat = 40.740676,
                LocTime = DateTime.Now,
                LocDesc = "Apcurium",
                Init = false,
                Type = PreCogType.Broadcast
            };
            sut.Send(statusRequest, false);
        }

        [Test]
        public void when_sending_guide()
        {

            var sut = new CmtPreCogServiceClient(BaseUrl, Credentials);
            var statusRequest = new PreCogRequest
            {
                LocLon = -74.004593,
                LocLat = 40.740676,
                LocTime = DateTime.Now,
                LocDesc = "Apcurium",
                Init = false,
                Type = PreCogType.Guide,
                DestDesc = "restaurant",
                DestLat = 40.738758,
                DestLon = -73.982706
            };
            sut.Send(statusRequest, false);
        }

    }
}
