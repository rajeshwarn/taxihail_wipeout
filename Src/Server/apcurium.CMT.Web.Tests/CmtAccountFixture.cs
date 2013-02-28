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
        public void when_registering_a_new_account_no_error()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);
            var email = "matthieu2@live.com";
            var newAccount = new RegisterAccount { Phone = "+15146543024", Email = email,Name = "Matthieu Guyonnet-Duluc", Password = "password" };
            sut.RegisterAccount(newAccount);

            Assert.Pass();
        }

        [Test]
        public void when_registering_an_existing_account_got_axception()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);
            var newAccount = new RegisterAccount { Phone = "+15146543024", Email = "matthieu@live.com", Name = "Matthieu Guyonnet-Duluc", Password = "password" };

            var exception = Assert.Throws<WebServiceException>(() => sut.RegisterAccount(newAccount));
        }

        [Test]
        public void when_authenticate_an_existing_account_can_get_my_account()
        {
            Authenticate();

            var myAccount = new CmtAccountServiceClient(BaseUrl, Credentials).GetMyAccount();

            Assert.IsNotNull(myAccount);
        }

        [Test]
        public void UpdateBookingSettingsAccountTest()
        {
            Authenticate();

            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);
            var myAccount = new CmtAccountServiceClient(BaseUrl, Credentials).GetMyAccount();

            var firstName = ("Mat" + Guid.NewGuid()).Substring(0,30);


            sut.UpdateBookingSettings(new BookingSettingsRequest
                {
                    Name = firstName + " " + myAccount.LastName,
                    Phone = myAccount.Phone
                });

            myAccount = new CmtAccountServiceClient(BaseUrl, Credentials).GetMyAccount();

            Assert.AreEqual(firstName, myAccount.FirstName);
        }

    }
}
