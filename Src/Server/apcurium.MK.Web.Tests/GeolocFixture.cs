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
            var sut = new GeocodingServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var addresses = sut.Search("11 hines");
            Assert.True(addresses.Addresses.Count() == 1);
            Assert.True(addresses.Addresses.ElementAt(0).FullAddress.Contains( "11" ));
        }

        [Test]
        public void BasicCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var addresses = sut.Search( 45.5062, -73.5726);
            Assert.True(addresses.Addresses.Count() >= 1);            
        }


        [Test]
        public void RangeCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var addresses = sut.Search(45.5227967351675, -73.6242310144007);
            Assert.True(addresses.Addresses.Count() >= 1);
        }


    



        


    }
}
