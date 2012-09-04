using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class GeolocFixture : BaseTest
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
        public void BasicNameSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId);
            var addresses = sut.Search("11 hines");
            Assert.True(addresses.Addresses.Count() == 1);
            Assert.True(addresses.Addresses.ElementAt(0).FullAddress.Contains( "11" ));
        }

        [Test]
        public void AdvancedNameSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId);
            var addresses = sut.Search("2100 Research Rd, Ottawa MacDonald-Cartier Airport (YOW), Gloucester, ON K1V 2B1, Canada");
            Assert.AreEqual(1, addresses.Addresses.Count());
            Assert.AreEqual(true, addresses.Addresses.ElementAt(0).FullAddress.Contains("Research"));
        }

        [Test]
        public void BasicCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId);
            var addresses = sut.Search( 45.5062, -73.5726);
            Assert.True(addresses.Addresses.Count() >= 1);            
        }


        [Test]
        public void RangeCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId);
            var addresses = sut.Search(45.5227967351675, -73.6242310144007);
            Assert.True(addresses.Addresses.Count() >= 1);
        }

        [Test]
        public void Airport_Coordinate_Search_And_Name_Search()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId);
            var addresses = sut.Search(45.326874, -75.672616);
            Assert.AreEqual(1, addresses.Addresses.Count());

            addresses = sut.Search(addresses.Addresses[0].FullAddress);
            Assert.AreEqual(1, addresses.Addresses.Count());
            Assert.AreEqual(true, addresses.Addresses.ElementAt(0).FullAddress.Contains("Convair"));
        }
    



        


    }
}
