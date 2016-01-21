using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class PlaceDetailFixture : BaseTest
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
        [Ignore("Use only for debugging purpose")]
        public async void when_place_then_detail()
        {
            try
            {
                var refService = new SearchLocationsServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
                var addresses = await refService.Search("yul", 45.4982111, -73.6565887);
                
                if (!addresses.Any())
                {
                    Assert.Inconclusive("no places returned");
                }

                var a1 = addresses.ElementAt(0);
                Assert.IsNotNullOrEmpty(a1.PlaceId);

                var sut = new PlaceDetailServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
                var address = await sut.GetPlaceDetail(a1.PlaceId, a1.FriendlyName);
                Assert.AreNotEqual(0, address.Latitude);
                Assert.AreNotEqual(0, address.Longitude);
                Assert.IsNotNullOrEmpty(address.FullAddress);
                Assert.IsNotNullOrEmpty(address.Street);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}