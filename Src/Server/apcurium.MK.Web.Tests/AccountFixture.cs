using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Resources;


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
            Assert.AreEqual(acc.Name, TestAccount.Name);            
            Assert.AreEqual(acc.Phone, TestAccount.Phone);

        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Invalid UserName or Password")]
        public void BasicSignInWithInvalidPassword()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, "wrong_password"));
            var acc = sut.GetMyAccount();
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Invalid UserName or Password")]
        public void BasicSignInWithInvalidEmail()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo("wrong_email@wrong.com", "password1"));
            var acc = sut.GetMyAccount();
        }


        [Test]
        public void RegisteringAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);


            sut = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, newAccount.Password));
            var account = sut.GetMyAccount();
            Assert.IsNotNull(account);
            Assert.AreEqual(newAccount.AccountId, account.Id);
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateAccount_AccountAlreadyExist")]
        public void when_registering_2_account_with_same_email()
        {
            string email = GetTempEmail();

            var sut = new AccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);


            var sut2 = new AccountServiceClient(BaseUrl, null);
            var newAccount2 = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);


        }

        [Test]
        public void when_resetting_account_password()
        {
            var email = GetTempEmail();
            var password = "password";

            var sut = new AccountServiceClient(BaseUrl, null);

            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = password };
            sut.RegisterAccount(newAccount);

            sut = new AccountServiceClient(BaseUrl, new AuthInfo(email, password));
            sut.ResetPassword(email);

            sut = new AccountServiceClient(BaseUrl, new AuthInfo(email, password));
            Assert.Throws<WebServiceException>(() => sut.GetMyAccount());
        }

        [Test]
        public void when_resetting_password_with_unknown_email_address()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var exception = Assert.Throws<WebServiceException>(() => sut.ResetPassword("this.is.not@my.email.addre.ss"));
            Assert.AreEqual(404, exception.StatusCode);
        }


        [Test]
        public void UpdateBookingSettingsAccountTest()
        {
            var settings = new BookingSettingsRequest
            {
                ChargeTypeId = 3,
                Name = "toto",                
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
            Assert.AreEqual(settings.Name, account.Settings.Name);
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
