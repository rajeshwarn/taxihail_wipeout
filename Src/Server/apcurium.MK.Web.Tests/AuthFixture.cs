using System;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;

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
        public async void when_user_sign_in()
        {
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            var response = await sut.Authenticate(TestAccount.Email, TestAccountPassword);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(TestAccount.Email, response.UserName);
        }

        [Test]
        public void when_user_sign_in_with_invalid_password()
        {
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            Assert.Throws<WebServiceException>(async () => await sut.Authenticate(TestAccount.Email, "wrong password"), "InvalidLoginMessage");
        }

        [Test]
        public void when_user_sign_in_with_invalid_email()
        {
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            Assert.Throws<WebServiceException>(async () => await sut.Authenticate("wrong_email@wrong.com", TestAccountPassword), "InvalidLoginMessage");
        }

        [Test]
        public async void when_user_sign_in_with_facebook()
        {
            var account = await GetNewFacebookAccount();
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            var response = await sut.AuthenticateFacebook(account.FacebookId);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(account.FacebookId, response.UserName);
        }

        [Test]
        public void when_user_sign_in_with_invalid_facebook_id()
        {
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            Assert.Throws<WebServiceException>(async () => await sut.AuthenticateFacebook(Guid.NewGuid().ToString()), "Invalid UserName or Password");
        }

        [Test]
        public async void when_user_sign_in_with_twitter()
        {
            var account = await GetNewTwitterAccount();
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            var response = await sut.AuthenticateTwitter(account.TwitterId);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId, "Test");
            Assert.AreEqual(account.TwitterId, response.UserName);
        }

        [Test]
        public void when_user_sign_in_with_invalid_twitter_id()
        {
            var sut = new AuthServiceClient(BaseUrl, null, new DummyPackageInfo());
            Assert.Throws<WebServiceException>(async () => await sut.AuthenticateTwitter(Guid.NewGuid().ToString()), "Invalid UserName or Password");
        }
    }
}
