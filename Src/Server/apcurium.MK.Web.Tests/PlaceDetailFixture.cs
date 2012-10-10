﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
  
    [TestFixture]
    public class PlaceDetailFixture : BaseTest
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
        public void when_place_then_detail()
        {
            var refService = new SearchLocationsServiceClient(BaseUrl, SessionId);
            var addresses = refService.Search("yul", 45.5227967351675, -73.6242310144007);

            var a1 = addresses.ElementAt(0);
            Assert.IsNotNullOrEmpty(a1.PlaceReference);

            var sut = new PlaceDetailServiceClient(BaseUrl, SessionId);
            var address = sut.GetPlaceDetail(a1.PlaceReference);
            Assert.AreNotEqual( 0,  address.Latitude ); 
            Assert.AreNotEqual( 0, address.Longitude );
            Assert.IsNotNullOrEmpty( address.FullAddress );
            Assert.IsNotNullOrEmpty( address.Street );            
            
        }

     

    }
}
