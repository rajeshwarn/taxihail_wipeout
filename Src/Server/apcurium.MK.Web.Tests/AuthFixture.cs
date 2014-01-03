#region

using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

#endregion

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
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            var response = sut.Authenticate(TestAccount.Email, TestAccountPassword);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(TestAccount.Email, response.UserName);
        }


        [Test]
        public void when_user_sign_in_with_facebook()
        {
            var account = GetNewFacebookAccount();
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            var response = sut.AuthenticateFacebook(account.FacebookId);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(account.FacebookId, response.UserName);
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "InvalidLoginMessage"
            )]
        public void when_user_sign_in_with_invalid_email()
        {
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            sut.Authenticate("wrong_email@wrong.com", TestAccountPassword);
        }

        [Test]
        public void when_user_sign_in_with_invalid_facebook_id()
        {
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.Throws<WebServiceException>(() => sut
                .AuthenticateFacebook(Guid.NewGuid().ToString()), "Invalid UserName or Password");
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "InvalidLoginMessage"
            )]
        public void when_user_sign_in_with_invalid_password()
        {
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            sut.Authenticate(TestAccount.Email, "wrong password");
        }

        [Test]
        public void when_user_sign_in_with_invalid_twitter_id()
        {
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.Throws<WebServiceException>(() => sut
                .AuthenticateTwitter(Guid.NewGuid().ToString()), "Invalid UserName or Password");
        }

        [Test]
        public void when_user_sign_in_with_twitter()
        {
            var account = GetNewTwitterAccount();
            var sut = new AuthServiceClient(BaseUrl, null, "Test");
            var response = sut.AuthenticateTwitter(account.TwitterId);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(account.TwitterId, response.UserName);
        }
    }
}