using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class AccountFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

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
        public async void RegisteringFacebookAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = Guid.NewGuid().ToString(),
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            var auth = await new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateFacebook(newAccount.FacebookId);
            var account = await sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.FacebookId, auth.UserName);
            Assert.IsNull(account.TwitterId);
        }

        [Test]
        public async void RegisteringTwitterAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = Guid.NewGuid().ToString(),
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            var auth = await new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateTwitter(newAccount.TwitterId);
            sut = new AccountServiceClient(BaseUrl, auth.SessionId, "Test");
            var account = await sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.TwitterId, auth.UserName);
            Assert.IsNull(account.FacebookId);
        }

        [Test]
        public async void UpdateBookingSettingsAccountTest()
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

            await sut.UpdateBookingSettings(settings);

            var account = await sut.GetMyAccount();

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

        [Test]
        public async void registering_account_and_confirm_by_admin_then_is_confirmed()
        {
            var email = GetTempEmail();

            var client = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };
            await client.RegisterAccount(newAccount);

            await CreateAndAuthenticateTestAdminAccount();

            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            sut.EnableAccount(new EnableAccountByAdminRequest {AccountEmail = email});

            var auth = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.DoesNotThrow(() => auth.Authenticate(email, "password"));
        }

        [Test]
        public async void registering_account_confirm_by_admin_and_disable_by_admin_then_is_not_confirmed()
        {
            var email = GetTempEmail();

            var client = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };
            await client.RegisterAccount(newAccount);

            await CreateAndAuthenticateTestAdminAccount();

            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            sut.EnableAccount(new EnableAccountByAdminRequest {AccountEmail = email});
            sut.DisableAccount(new DisableAccountByAdminRequest {AccountEmail = email});

            var auth = new AuthServiceClient(BaseUrl, null, "Test");
            Assert.Throws<WebServiceException>(async () => await auth.Authenticate(email, "password"));
        }

        [Test]
        public async void registering_account_has_settings()
        {
            // Arrange
            await CreateAndAuthenticateTestAccount();

            // Act
            var account = await new AccountServiceClient(BaseUrl, SessionId, "Test").GetMyAccount();

            // Assert
            Assert.AreEqual("en", account.Language);
            Assert.IsNotNull(account.Settings);
            Assert.AreEqual(account.Settings.Name, account.Name);
            Assert.AreEqual(account.Settings.Phone, account.Phone);
        }

        [Test]
        public async void when_getting_user_account()
        {
            var account = await new AccountServiceClient(BaseUrl, SessionId, "Test").GetMyAccount();

            Assert.IsNotNull(account);
            Assert.AreEqual(account.Id, TestAccount.Id);
            Assert.AreEqual(account.Email, TestAccount.Email);
            Assert.AreEqual(account.Name, TestAccount.Name);
            Assert.AreEqual(account.Phone, TestAccount.Phone);
        }

        [Test]
        public async void when_granting_admin_access()
        {
            var fbAccount = await GetNewFacebookAccount();
            await CreateAndAuthenticateTestAdminAccount();
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");

            Assert.DoesNotThrow(() => sut.GrantAdminAccess(new GrantAdminRightRequest {AccountEmail = fbAccount.Email}));
        }

        [Test]
        public async void when_granting_admin_access_with_incorrect_rights()
        {
            var asc = new AccountServiceClient(BaseUrl, null, "Test");

            var fbAccount = await GetNewFacebookAccount();

            var newAccount = await asc.CreateTestAccount();
            await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, TestAccountPassword);
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, "Test");
            Assert.Throws<WebServiceException>(() => sut.GrantAdminAccess(new GrantAdminRightRequest {AccountEmail = fbAccount.Email}));
        }

        [Test]
        public async void when_registering_2_account_with_same_email()
        {
            var email = GetTempEmail();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };
            Assert.Throws<WebServiceException>(async () => await sut.RegisterAccount(newAccount2), "CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async void when_registering_2_account_with_same_facebookId()
        {
            var facebookId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = facebookId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = facebookId.ToString()
            };

            Assert.Throws<WebServiceException>(async () => await sut.RegisterAccount(newAccount2), "CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async void when_registering_2_account_with_same_twitterId()
        {
            var twitterId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };

            Assert.Throws<WebServiceException>(async () => await sut.RegisterAccount(newAccount2), "CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async void when_registering_a_new_account()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                Password = "password",
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            Assert.Throws<WebServiceException>(async () => await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, newAccount.Password));
        }

        [Test]
        public async void when_resetting_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var newAccount = await sut.CreateTestAccount();
            await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(newAccount.Email, TestAccountPassword);

            await sut.ResetPassword(newAccount.Email);

            Assert.Throws<WebServiceException>(async () => await sut.GetMyAccount());
        }

        [Test]
        public void when_resetting_password_with_unknown_email_address()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var exception = Assert.Throws<WebServiceException>(async () => await sut.ResetPassword("this.is.not@my.email.addre.ss"));
            Assert.AreEqual(500, exception.StatusCode);
        }

        [Test]
        public async void when_updating_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = await CreateAndAuthenticateTestAccount();

            await sut.UpdatePassword(new UpdatePassword
                {
                    AccountId = account.Id,
                    CurrentPassword = TestAccountPassword,
                    NewPassword = "p@55w0rddddddddd"
                });

            Assert.DoesNotThrow(() => new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, "p@55w0rddddddddd"));
        }

        [Test]
        [Ignore]
        public async void when_updating_account_password__user_is_logout()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = await CreateAndAuthenticateTestAccount();

            await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, TestAccountPassword);
            var request = new UpdatePassword
            {
                AccountId = account.Id,
                CurrentPassword = TestAccountPassword,
                NewPassword = "p@55w0rddddddddd"
            };
            await sut.UpdatePassword(request);
            Assert.Throws<WebServiceException>(async () => await sut.GetFavoriteAddresses());
        }

        [Test]
        public async void when_updating_account_password_with_wrong_current_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");

            var account = await CreateAndAuthenticateTestAccount();

            await new AuthServiceClient(BaseUrl, SessionId, "Test").Authenticate(account.Email, TestAccountPassword);
            var request = new UpdatePassword
            {
                AccountId = account.Id,
                CurrentPassword = "wrongpassword",
                NewPassword = "p@55w0rddddddddd"
            };

            Assert.Throws<WebServiceException>(async () => await sut.UpdatePassword(request));
        }

        [Test]
        public async void when_updating_twitter_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test");
            const string password = "yop";
            var accountId = Guid.NewGuid();
            var twitterId = Guid.NewGuid();

            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5146543024",
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            await new AuthServiceClient(BaseUrl, SessionId, "Test").AuthenticateTwitter(newAccount.TwitterId);
            var request = new UpdatePassword
            {
                AccountId = accountId,
                CurrentPassword = password,
                NewPassword = "p@55w0rddddddddd"
            };
            Assert.Throws<WebServiceException>(async () => await sut.UpdatePassword(request));
        }
            
    }
}