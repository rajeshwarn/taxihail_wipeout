using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;


namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class AccountFixture : BaseTest
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
        public void BasicSignIn()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var acc = sut.GetMyAccount();
            
            Assert.IsNotNull(acc);
            Assert.AreEqual(acc.Id, TestAccount.Id);
            Assert.AreEqual(acc.Email, TestAccount.Email);
            Assert.AreEqual(acc.FirstName, TestAccount.FirstName);
            Assert.AreEqual(acc.LastName, TestAccount.LastName);
            Assert.AreEqual(acc.Phone, TestAccount.Phone);
            
        }

        [Test]
        [ExpectedException( "ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage="Unauthorized")]
        public void BasicSignInWithInvalidPassword()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, "wrong_password"));
            var acc = sut.GetMyAccount();            
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Unauthorized")]
        public void BasicSignInWithInvalidEmail()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo("wrong_email@wrong.com", "password1"));
            var acc = sut.GetMyAccount();
        }


        [Test]
        public void RegisteringAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone =  "5146543024", Email = GetTempEmail(), FirstName = "First Name Test", LastName = "Last Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);
            Thread.Sleep(400);
            sut = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, newAccount.Password));
            var account = sut.GetMyAccount();
            Assert.IsNotNull(account);
            Assert.AreEqual(newAccount.AccountId, account.Id);
        }


        [Test]
        public void UpdateBookingSettingsAccountTest()
        {
            var settings = new BookingSettingsRequest
            {
                ChargeTypeId = 3,
                FirstName = "toto",
                LastName = "titi",
                NumberOfTaxi = 6,
                Passengers = 8,
                Phone = "12345",
                ProviderId = 85,
                VehicleTypeId = 92
            };

            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            sut.UpdateBookingSettings(TestAccount.Id, settings);

            var account = sut.GetMyAccount();
            
            Assert.AreEqual(settings.ChargeTypeId, account.Settings.ChargeTypeId);
            Assert.AreEqual(settings.FirstName, account.Settings.FirstName);
            Assert.AreEqual(settings.NumberOfTaxi, account.Settings.NumberOfTaxi);
            Assert.AreEqual(settings.Passengers, account.Settings.Passengers);
            Assert.AreEqual(settings.Phone, account.Settings.Phone);
            Assert.AreEqual(settings.ProviderId, account.Settings.ProviderId);
            Assert.AreEqual(settings.VehicleTypeId, account.Settings.VehicleTypeId);
        }


        private string GetTempEmail()
        {
            var email = string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
            return email;
        }



       

    }
}
