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
        public async void when_place_then_detail()
        {
            try
            {
                var refService = new SearchLocationsServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
                var addresses = await refService.Search("yul", 45.5227967351675, -73.6242310144007);
                
                if (!addresses.Any())
                {
                    Assert.Inconclusive("no places returned");
                }

                var a1 = addresses.ElementAt(0);
                Assert.IsNotNullOrEmpty(a1.PlaceReference);

                var sut = new PlaceDetailServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
                var address = await sut.GetPlaceDetail(a1.PlaceReference, a1.FriendlyName);
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