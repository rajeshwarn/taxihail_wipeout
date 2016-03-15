﻿using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Http.Exceptions;
using MK.Common.Exceptions;
using NUnit.Framework;

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
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = Guid.NewGuid().ToString(),
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            var auth = await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).AuthenticateFacebook(newAccount.FacebookId);
            sut = new AccountServiceClient(BaseUrl, auth.SessionId, new DummyPackageInfo(), null, null);
            var account = await sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.FacebookId, auth.UserName);
            Assert.IsNull(account.TwitterId);
        }

        [Test]
        public async void RegisteringTwitterAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Email = GetTempEmail(),
                Country = new CountryISOCode("CA"),
                Name = "First Name Test",
                TwitterId = Guid.NewGuid().ToString(),
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            var auth = await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).AuthenticateTwitter(newAccount.TwitterId);
            sut = new AccountServiceClient(BaseUrl, auth.SessionId, new DummyPackageInfo(), null, null);
            var account = await sut.GetMyAccount();
            Assert.IsNotNull(auth);
            Assert.AreEqual(newAccount.TwitterId, auth.UserName);
            Assert.IsNull(account.FacebookId);
        }

        [Test]
        public async void UpdateBookingSettingsAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var initialAccount = await sut.GetMyAccount();

            var settings = new BookingSettingsRequest
            {
                ChargeTypeId = ChargeTypes.CardOnFile.Id,
                Name = "toto",
                NumberOfTaxi = 6,
                Passengers = 8,
                Phone = "5141234567",
                Country = new CountryISOCode("CA"),
                ProviderId = 13,
                VehicleTypeId = 1,
                DefaultTipPercent = 15,
                Email = initialAccount.Email
            };

            await sut.UpdateBookingSettings(settings);

            var updatedAccount = await sut.GetMyAccount();

            Assert.AreEqual(settings.ChargeTypeId, updatedAccount.Settings.ChargeTypeId);
            Assert.AreEqual(settings.Name, updatedAccount.Settings.Name);
            Assert.AreEqual(settings.NumberOfTaxi, updatedAccount.Settings.NumberOfTaxi);
            Assert.AreEqual(settings.Passengers, updatedAccount.Settings.Passengers);
            Assert.AreEqual(settings.Phone, updatedAccount.Settings.Phone);
            Assert.AreEqual(settings.ProviderId, updatedAccount.Settings.ProviderId);
            Assert.AreEqual(settings.VehicleTypeId, updatedAccount.Settings.VehicleTypeId);
            Assert.AreEqual(settings.DefaultTipPercent, updatedAccount.DefaultTipPercent);
            Assert.AreEqual(settings.AccountNumber, updatedAccount.Settings.AccountNumber);
            Assert.AreEqual(settings.CustomerNumber, updatedAccount.Settings.CustomerNumber);
        }

        [Test]
        [Ignore("It now relies on a payment setting which we can't really change here since this is client side and we don't have a service client for server payment settings")]
        public async Task Update_Booking_Settings_With_Invalid_Charge_Account_Test_Then_Exception_Thrown()
        {
            var settings = new BookingSettingsRequest
            {
                ChargeTypeId = ChargeTypes.CardOnFile.Id,
                Name = "toto",
                NumberOfTaxi = 6,
                Passengers = 8,
                Phone = "12345",
                ProviderId = 13,
                VehicleTypeId = 1,
                DefaultTipPercent = 15,
                AccountNumber = "IDONOTEXIST",
                CustomerNumber = "0"
            };

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            try
            {
                await sut.UpdateBookingSettings(settings);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async void registering_account_has_settings()
        {
            // Arrange
            await CreateAndAuthenticateTestAccount();

            // Act
            var account = await new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).GetMyAccount();

            // Assert
            Assert.AreEqual("en", account.Language);
            Assert.IsNotNull(account.Settings);
            Assert.AreEqual(account.Settings.Name, account.Name);
        }

        [Test]
        public async void when_getting_user_account()
        {
            var account = await new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).GetMyAccount();

            Assert.IsNotNull(account);
            Assert.AreEqual(account.Id, TestAccount.Id);
            Assert.AreEqual(account.Email, TestAccount.Email);
            Assert.AreEqual(account.Name, TestAccount.Name);
            Assert.AreEqual(account.Phone, TestAccount.Phone);
            Assert.AreEqual(account.Language, TestAccount.Language);
        }

        [Test]
        public async Task when_granting_admin_access()
        {
            var fbAccount = await GetNewFacebookAccount();
            await CreateAndAuthenticateTestAdminAccount();
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            try
            {
                await sut.GrantAdminAccess(new GrantAdminRightRequest {AccountEmail = fbAccount.Email});
            }
            catch (Exception ex)
            {
                Assert.DoesNotThrow(() =>
                {
                    throw ex;
                });
            }
            Assert.Pass();
        }

        [Test]
        public async Task when_granting_admin_access_with_incorrect_rights()
        {
            var asc = new AccountServiceClient(BaseUrl, null, new DummyPackageInfo(), null, null);

            var fbAccount = await GetNewFacebookAccount();

            var newAccount = await asc.CreateTestAccount();
            await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(newAccount.Email, TestAccountPassword);
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
            
            try
            {
                await sut.GrantAdminAccess(new GrantAdminRightRequest { AccountEmail = fbAccount.Email });
            }
            catch (Exception ex)
            {
                Assert.Throws<ServiceResponseException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async Task when_registering_2_account_with_same_email()
        {
            var email = GetTempEmail();

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = email,
                Name = "First Name Test",
                Password = "password"
            };

            try
            {
                await sut.RegisterAccount(newAccount2);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                }, "CreateAccount_AccountAlreadyExist");
            }
            
            Assert.Fail("CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async Task when_registering_2_account_with_same_facebookId()
        {
            var facebookId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = facebookId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                FacebookId = facebookId.ToString()
            };

            try
            {
                await sut.RegisterAccount(newAccount2);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                }, "CreateAccount_AccountAlreadyExist");
                throw;
            }

            Assert.Fail("CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async Task when_registering_2_account_with_same_twitterId()
        {
            var twitterId = Guid.NewGuid();

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            var newAccount2 = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };

            try
            {
                await sut.RegisterAccount(newAccount2);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                }, "CreateAccount_AccountAlreadyExist");
                throw;
            }

            Assert.Fail("CreateAccount_AccountAlreadyExist");
        }

        [Test]
        public async Task when_registering_a_new_account()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                Password = "password",
                Language = "en"
            };
            await sut.RegisterAccount(newAccount);

            var authService = new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            try
            {
                await authService.Authenticate(newAccount.Email, newAccount.Password);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async Task when_resetting_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var newAccount = await sut.CreateTestAccount();
            await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(newAccount.Email, TestAccountPassword);

            await sut.ResetPassword(newAccount.Email);

            try
            {
                await sut.GetMyAccount();
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async Task when_resetting_password_with_unknown_email_address()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            try
            {
                await sut.ResetPassword("this.is.not@my.email.addre.ss");
            }
            catch (Exception ex)
            {
                var exception = Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });

                Assert.AreEqual(500, exception.StatusCode);
            }

            Assert.Fail();
        }

        [Test]
        public async Task when_updating_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var account = await CreateAndAuthenticateTestAccount();

            await sut.UpdatePassword(new UpdatePassword
            {
                AccountId = account.Id,
                CurrentPassword = TestAccountPassword,
                NewPassword = "p@55w0rddddddddd"
            });

            try
            {
                await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(account.Email, "p@55w0rddddddddd");
            }
            catch (Exception ex)
            {
                Assert.DoesNotThrow(() =>
                {
                    throw ex;
                });
            }

            Assert.Pass();
        }

        [Test]
        [Ignore]
        public async Task when_updating_account_password__user_is_logout()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var account = await CreateAndAuthenticateTestAccount();

            await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(account.Email, TestAccountPassword);
            var request = new UpdatePassword
            {
                AccountId = account.Id,
                CurrentPassword = TestAccountPassword,
                NewPassword = "p@55w0rddddddddd"
            };
            await sut.UpdatePassword(request);

            try
            {
                await sut.GetFavoriteAddresses();
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async Task when_updating_account_password_with_wrong_current_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);

            var account = await CreateAndAuthenticateTestAccount();

            await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).Authenticate(account.Email, TestAccountPassword);
            var request = new UpdatePassword
            {
                AccountId = account.Id,
                CurrentPassword = "wrongpassword",
                NewPassword = "p@55w0rddddddddd"
            };

            try
            {
                await sut.UpdatePassword(request);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }

        [Test]
        public async void when_updating_twitter_account_password()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null);
            const string password = "yop";
            var accountId = Guid.NewGuid();
            var twitterId = Guid.NewGuid();

            var newAccount = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Phone = "5145551234",
                Country = new CountryISOCode("CA"),
                Email = GetTempEmail(),
                Name = "First Name Test",
                TwitterId = twitterId.ToString()
            };
            await sut.RegisterAccount(newAccount);

            await new AuthServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null, null).AuthenticateTwitter(newAccount.TwitterId);
            var request = new UpdatePassword
            {
                AccountId = accountId,
                CurrentPassword = password,
                NewPassword = "p@55w0rddddddddd"
            };

            try
            {
                await sut.UpdatePassword(request);
            }
            catch (Exception ex)
            {
                Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });
            }

            Assert.Fail();
        }
    }
}