using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class DirectionFixture : BaseTest
    {

        private const string _testUserEmail = "apcurium.test@apcurium.com";
        private const string _testUserPassword = "password";


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
        public void BasicDirectionSearch()
        {
            var sut = new DirectionsServiceClient(BaseUrl, SessionId, "Test");
            var direction = sut.GetDirectionDistance(45.5062, -73.5726, 45.5273, -73.6344);

            Assert.IsNotNull(direction);
            Assert.True(direction.Distance.HasValue);
            Assert.True(direction.Price.HasValue);
            
        }









    }
}
