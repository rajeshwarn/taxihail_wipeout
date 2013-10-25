using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;

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
        public void DefaultLocationIsAnAddress()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var address = sut.DefaultLocation();

            Assert.IsNotNull(address);
            Assert.AreEqual(45.516667, address.Latitude);
            Assert.AreEqual(-73.65, address.Longitude);
        }

        [Test]
        public void BasicNameSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var addresses = sut.Search("11 hines");
            Assert.True(addresses.Count() == 1);
            Assert.True(addresses.ElementAt(0).FullAddress.Contains( "11" ));
        }

        [Test]
        public void AdvancedNameSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var addresses = sut.Search("5661 avenue chateaubriand, Montreal");
            Assert.AreEqual(2, addresses.Count());
            var address = addresses.ElementAt(0);
            Assert.AreEqual(true, address.FullAddress.Contains("Chateaubriand"));
            Assert.AreEqual("5661", address.StreetNumber);
            Assert.AreEqual("Avenue de Chateaubriand", address.Street);
            Assert.AreEqual("Montreal", address.City);
            StringAssert.Contains("H2S", address.ZipCode);
        }

    
        [Test]
        public void BasicCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var addresses = sut.Search( 45.5062, -73.5726);
            Assert.True(addresses.Count() >= 1);            
        }


        [Test]
        public void RangeCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var addresses = sut.Search(45.5364, -73.6103);
            Assert.True(addresses.Count() >= 1);

            Assert.False(addresses.First().StreetNumber.Contains("-"));
            Assert.False(addresses.First().FullAddress.Split(' ')[0].Contains("-"));
        }

        [Test]
        public void SearchMiddleField()
        {
            var sut = new GeocodingServiceClient(BaseUrl, SessionId, "Test");
            var addresses = sut.Search(45.5364, -73.6103);                     
                        

            Assert.True(addresses.Count() >= 1);

            Assert.False(addresses.First().StreetNumber.Contains("-"));
            Assert.False(addresses.First().FullAddress.Split(' ')[0].Contains("-"));
        }
    }
}
