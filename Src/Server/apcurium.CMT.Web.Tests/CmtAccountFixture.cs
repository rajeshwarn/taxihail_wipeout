using System;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;


namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class CmtAccountFixture : CmtBaseTest
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
        public void when_registering_a_new_account()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { Phone = "5146543024", Email = GetTempEmail(),FirstName = "FirstName Test",LastName = "LastName Test", Password = "password" };
            sut.RegisterAccount(newAccount);

            Assert.Throws<WebServiceException>(() => new AuthServiceClient(BaseUrl, SessionId).Authenticate(newAccount.Email, newAccount.Password));
        }

    }
}
