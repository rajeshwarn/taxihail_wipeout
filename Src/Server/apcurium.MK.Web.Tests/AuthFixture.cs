﻿using System;
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
            var sut = new AuthServiceClient(BaseUrl);
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
            var sut = new AuthServiceClient(BaseUrl);
            var response = sut.Authenticate(TestAccount.Email, "wrong password");
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Invalid UserName or Password")]
        public void when_user_sign_in_with_invalid_email()
        {
            var sut = new AuthServiceClient(BaseUrl);
            var response = sut.Authenticate("wrong_email@wrong.com", TestAccountPassword);
        }

        [Test]
        public void when_user_sign_in_with_facebook()
        {
            var facebookId = "1234";
            var sut = new AuthServiceClient(BaseUrl);
            var response = sut.AuthenticateFacebook(facebookId);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId);
            Assert.AreEqual(facebookId, response.UserName);
        }
    }
}
