using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class AuthFixture : BaseTest
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


        [Test]
        public void when_user_sign_in()
        {
            var sut = new AuthServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var response = sut.Authenticate(TestAccount.Email, TestAccountPassword);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId);
            Assert.AreEqual(TestAccount.Email, response.UserName);
        }


        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException",
            ExpectedMessage = "Invalid UserName or Password")]
        public void when_user_sign_in_with_invalid_password()
        {
            var sut = new AuthServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, "wrong_password"));
            var response = sut.Authenticate(TestAccount.Email, "wrong password");
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Invalid UserName or Password")]
        public void when_user_sign_in_with_invalid_email()
        {
            var sut = new AuthServiceClient(BaseUrl, new AuthInfo("wrong_email@wrong.com", "password1"));
            var response = sut.Authenticate("wrong_email@wrong.com", TestAccountPassword);
        }
    }
}
