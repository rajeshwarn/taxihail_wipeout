using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;


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
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
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
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", Password = "password", Language = "en" };
            sut.RegisterAccount(newAccount);

            Assert.Throws<WebServiceException>(() => new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, newAccount.Password));
        }
        
        [Test]
        public void RegisteringFacebookAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = Guid.NewGuid().ToString(), Language = "en" };
            sut.RegisterAccount(newAccount);

            var auth = new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateFacebook(newAccount.FacebookId);
            var account = sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.FacebookId, auth.UserName);
            Assert.IsNull(account.TwitterId);
        }

        [Test]
        public void RegisteringTwitterAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = Guid.NewGuid().ToString(), Language = "en" };
            sut.RegisterAccount(newAccount);

            var auth = new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateTwitter(newAccount.TwitterId);
            var account = sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.TwitterId, auth.UserName);
            Assert.IsNull(account.FacebookId);
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateAccount_AccountAlreadyExist")]
        public void when_registering_2_account_with_same_facebookId()
        {
            var facebookId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = facebookId.ToString() };
            sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = facebookId.ToString() };
            sut.RegisterAccount(newAccount2);
            
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateAccount_AccountAlreadyExist")]
        public void when_registering_2_account_with_same_twitterId()
        {
            var twitterId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = twitterId.ToString() };
            sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = twitterId.ToString() };
            sut.RegisterAccount(newAccount2);
        }


        [Test]
        public void registering_account_has_settings()
        {
            // Arrange
            CreateAndAuthenticateTestAccount();
            
            // Act
            var account = new AccountServiceClient(BaseUrl, SessionId, "Test").GetMyAccount();

            // Assert
            Assert.AreEqual("en", account.Language);
            Assert.IsNotNull(account.Settings);
            Assert.AreEqual(account.Settings.Name, account.Name);
            Assert.AreEqual(account.Settings.Phone, account.Phone);


        }

        [Test]
        public void registering_account_and_confirm_by_admin_then_is_confirmed()
        {
            string email = GetTempEmail();

            var client = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password"  };
            client.RegisterAccount(newAccount);

            CreateAndAuthenticateTestAdminAccount();

            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            sut.EnableAccount(new EnableAccountByAdminRequest{ AccountEmail = email });

            var auth = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.DoesNotThrow(() => auth.Authenticate(email, "password"));
        }

        [Test]
        public void registering_account_confirm_by_admin_and_disable_by_admin_then_is_not_confirmed()
        {
            string email = GetTempEmail();

            var client = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            client.RegisterAccount(newAccount);

            CreateAndAuthenticateTestAdminAccount();

            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            sut.EnableAccount(new EnableAccountByAdminRequest { AccountEmail = email });
            sut.DisableAccount(new DisableAccountByAdminRequest { AccountEmail = email });

            var auth = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.Throws<WebServiceException>(() => auth.Authenticate(email, "password"));
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "CreateAccount_AccountAlreadyExist")]
        public void when_registering_2_account_with_same_email()
        {
            string email = GetTempEmail();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password"  };
            sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = email, Name = "First Name Test", Password = "password" };
            sut.RegisterAccount(newAccount2);           
        }

        [Test]
        public void when_resetting_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = sut.CreateTestAccount();
            new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, TestAccountPassword);

            sut.ResetPassword(newAccount.Email);

            Assert.Throws<WebServiceException>(() => sut.GetMyAccount());
        }

        [Test]
        public void when_updating_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = CreateAndAuthenticateTestAccount();


            sut.UpdatePassword(new UpdatePassword()
                                   {
                                       AccountId = account.Id,
                                       CurrentPassword = base.TestAccountPassword,
                                       NewPassword = "p@55w0rddddddddd"
                                   });

            Assert.DoesNotThrow(() => new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, "p@55w0rddddddddd"));

        }
        [Test]
        public void when_updating_account_password_with_wrong_current_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = CreateAndAuthenticateTestAccount();

            new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, base.TestAccountPassword);
            var request = new UpdatePassword()
                              {
                                  AccountId = account.Id,
                                  CurrentPassword = "wrongpassword",
                                  NewPassword = "p@55w0rddddddddd"
                             };

            Assert.Throws<WebServiceException>(() => sut.UpdatePassword(request));
        }

        [Test]
        [Ignore]
        public void when_updating_account_password__user_is_logout()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = CreateAndAuthenticateTestAccount();

            new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, base.TestAccountPassword);
            var request = new UpdatePassword()
            {
                AccountId = account.Id,
                CurrentPassword = base.TestAccountPassword,
                NewPassword = "p@55w0rddddddddd"
            };
            sut.UpdatePassword(request);
            Assert.Throws<WebServiceException>(() => sut.GetFavoriteAddresses());
        }

        [Test]
        public void when_updating_twitter_account_password()
        {

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var password = "yop";
            var accountId = Guid.NewGuid();
            var twitterId = Guid.NewGuid();
            var newMail = GetTempEmail();

            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = twitterId.ToString() };
            sut.RegisterAccount(newAccount);

            var auth = new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateTwitter(newAccount.TwitterId);
            var request = new UpdatePassword()
            {
                AccountId = accountId,
                CurrentPassword = password,
                NewPassword = "p@55w0rddddddddd"
            };
            Assert.Throws<WebServiceException>(() => sut.UpdatePassword(request));
        }

        [Test]
        public void when_resetting_password_with_unknown_email_address()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var exception = Assert.Throws<WebServiceException>(() => sut.ResetPassword("this.is.not@my.email.addre.ss"));
            Assert.AreEqual(500, exception.StatusCode);
        }

        [Test]
        public void when_granting_admin_access()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            var fbAccount = this.GetNewFacebookAccount();
            CreateAndAuthenticateTestAdminAccount();
            sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            
            Assert.DoesNotThrow(() => sut.GrantAdminAccess(new GrantAdminRightRequest() { AccountEmail = fbAccount.Email }));

        }

        [Test]
        public void when_granting_admin_access_with_incorrect_rights()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            var asc = new AccountServiceClient(BaseUrl, null, "Test");

            var fbAccount = this.GetNewFacebookAccount();

            var newAccount = asc.CreateTestAccount();
            new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, TestAccountPassword);
             sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            Assert.Throws<WebServiceException>(() => sut.GrantAdminAccess(new GrantAdminRightRequest() {AccountEmail = fbAccount.Email}));

        }


        [Test]
        public void UpdateBookingSettingsAccountTest()
        {
            Guid? creditCardId = Guid.NewGuid();
            int? defaultTipPercent = 15;

            var settings = new BookingSettingsRequest
            {
                ChargeTypeId = 3,
                Name = "toto",                
                NumberOfTaxi = 6,
                Passengers = 8,
                Phone = "12345",
                ProviderId = 13,
                VehicleTypeId = 1,
                DefaultCreditCard = creditCardId,
                DefaultTipPercent = defaultTipPercent
            };

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            sut.UpdateBookingSettings(settings);

            var account = sut.GetMyAccount();

            Assert.AreEqual(settings.ChargeTypeId, account.Settings.ChargeTypeId);
            Assert.AreEqual(settings.Name, account.Settings.Name);
            Assert.AreEqual(settings.NumberOfTaxi, account.Settings.NumberOfTaxi);
            Assert.AreEqual(settings.Passengers, account.Settings.Passengers);
            Assert.AreEqual(settings.Phone, account.Settings.Phone);
            Assert.AreEqual(settings.ProviderId, account.Settings.ProviderId);
            Assert.AreEqual(settings.VehicleTypeId, account.Settings.VehicleTypeId);
            Assert.AreEqual(creditCardId, account.DefaultCreditCard);
            Assert.AreEqual(defaultTipPercent, account.DefaultTipPercent);
        }


    }
}
