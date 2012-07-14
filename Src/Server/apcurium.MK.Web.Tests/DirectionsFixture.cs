using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class DirectionFixture : BaseTest
    {

        private const string _testUserEmail = "apcurium.test@apcurium.com";
        private const string _testUserPassword = "password";


        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
            //sut = new AccountService();
        }

        [Test]
        public void BasicDirectionSearch()
        {
            var sut = new DirectionsServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var direction = sut.GetDirectionDistance(45.5062, -73.5726, 45.5273, -73.6344);

            Assert.IsNotNull(direction);
            Assert.True(direction.Distance.HasValue);
            Assert.True(direction.Price.HasValue);
            
        }









    }
}
