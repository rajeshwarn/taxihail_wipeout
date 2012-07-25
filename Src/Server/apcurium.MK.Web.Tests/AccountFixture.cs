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
        public void when_getting_user_account()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var acc = sut.GetMyAccount();

            Assert.IsNotNull(acc);
            Assert.AreEqual(acc.Id, TestAccount.Id);
            Assert.AreEqual(acc.Email, TestAccount.Email);
            Assert.AreEqual(acc.Name, TestAccount.Name);            
            Assert.AreEqual(acc.Phone, TestAccount.Phone);
        }
        

        [Test]
        public void when_registering_a_new_account()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);

            Assert.Throws<WebServiceException>(() => new AuthServiceClient(BaseUrl).Authenticate(newAccount.Email, newAccount.Password));
        }
        
        [Test]
        public void RegisteringFacebookAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = "123456789"};
            sut.RegisterAccount(newAccount);

            var auth = new AuthServiceClient(BaseUrl).AuthenticateFacebook(newAccount.FacebookId);
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.FacebookId, auth.UserName);
        }

        [Test]
        public void RegisteringTwitterAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = "123456789" };
            sut.RegisterAccount(newAccount);

            var auth = new AuthServiceClient(BaseUrl).AuthenticateTwitter(newAccount.TwitterId);
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.TwitterId, auth.UserName);
        }


        [Test]
        public void registering_account_has_settings()
        {
            // Arrange
            var sut = new AccountServiceClient(BaseUrl);
            
            // Act
            CreateAndAuthenticateTestAccount();
            var account = sut.GetMyAccount();

            // Assert
            Assert.IsNotNull(account.Settings);
            Assert.AreEqual(account.Settings.Name, account.Name);
            Assert.AreEqual(account.Settings.Phone, account.Phone);


        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateAccount_AccountAlreadyExist")]
        public void when_registering_2_account_with_same_email()
        {
            string email = GetTempEmail();

            var sut = new AccountServiceClient(BaseUrl);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount2);


        }

        [Test]
        public void when_resetting_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var newAccount = sut.CreateTestAccount();
            new AuthServiceClient(BaseUrl).Authenticate(newAccount.Email, TestAccountPassword);

            sut.ResetPassword(newAccount.Email);

            Assert.Throws<WebServiceException>(() => sut.GetMyAccount());
        }

        [Test]
        public void when_resetting_password_with_unknown_email_address()
        {
            var sut = new AccountServiceClient(BaseUrl);

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

            var sut = new AccountServiceClient(BaseUrl);

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


      




    }
}
