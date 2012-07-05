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
        public new void Setup()
        {
            base.Setup();
        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        } 

        [Test]
        public void BasicNameSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var addresses = sut.Search("5250 ferrier montreal");
            Assert.True(addresses.Addresses.Count() == 1);
            Assert.True(addresses.Addresses.ElementAt(0).FullAddress.Contains( "5250" ));
        }

        [Test]
        public void BasicCoordinateSearch()
        {
            var sut = new GeocodingServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var addresses = sut.Search( 45.5062, -73.5726);
            Assert.True(addresses.Addresses.Count() >= 1);            
        }


    



        


    }
}
