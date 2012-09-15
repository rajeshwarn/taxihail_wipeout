using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
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
            var addresses = sut.Search("5661 avenue chateaubriand, Montreal");
            Assert.AreEqual(2, addresses.Addresses.Count());
            var address = addresses.Addresses.ElementAt(0);
            Assert.AreEqual(true, address.FullAddress.Contains("Chateaubriand"));
            Assert.AreEqual("5661", address.StreetNumber);
            Assert.AreEqual("Avenue de Chateaubriand", address.Street);
            Assert.AreEqual("Montreal", address.City);
            Assert.AreEqual("H2S 0A4", address.ZipCode);
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

            Assert.False(addresses.Addresses.First().StreetNumber.Contains("-"));
            Assert.False(addresses.Addresses.First().FullAddress.Split(' ')[0].Contains("-"));
        }
    }
}
