#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected AccountDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new AccountDetailsGenerator(() => new BookingDbContext(DbName), new TestServerSettings());
        }
    }

    [TestFixture]
    public class given_no_account : given_a_view_model_generator
    {
        [Test]
        public void when_account_registered_then_account_dto_populated()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                Password = new byte[] {1},
                FacebookId = "FacebookId",
                TwitterId = "TwitterId",
                Language = "fr"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Bob", dto.Name);
                Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                Assert.AreEqual(1, dto.Password.Length);
                Assert.AreEqual("FacebookId", dto.FacebookId);
                Assert.AreEqual("TwitterId", dto.TwitterId);
                Assert.AreEqual(false, dto.IsConfirmed);
                Assert.AreEqual(false, dto.DisabledByAdmin);
                Assert.AreEqual("fr", dto.Language);
            }
        }

        [Test]
        public void when_account_registered_then_account_is_not_confirmed()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(false, dto.IsConfirmed);
                Assert.AreEqual(false, dto.DisabledByAdmin);
            }
        }

        [Test]
        public void when_account_registered_then_account_settings_populated()
        {
            var accountId = Guid.NewGuid();

            var accountRegistered = new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                Phone = "555.555.2525",
                Password = new byte[] {1}
            };
            Sut.Handle(accountRegistered);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(dto.Settings.Name, dto.Name);
                Assert.AreEqual(CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode.Code, dto.Settings.Country.Code);
                Assert.AreEqual(accountRegistered.Phone, dto.Settings.Phone);


                var config = new TestServerSettings();
                Assert.IsNull(dto.Settings.ChargeTypeId);
                Assert.AreEqual(dto.Settings.Passengers, config.ServerData.DefaultBookingSettings.NbPassenger);
                Assert.IsNull(dto.Settings.VehicleTypeId);
                Assert.IsNull(dto.Settings.ProviderId);
            }
        }

        [Test]
        public void when_account_registered_with_null_country_code()
        {
            var accountId = Guid.NewGuid();

            var accountRegistered = new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@apcurium.com",
                Country = null,
                Phone = "555.555.2525",
                Password = new byte[] { 1 }
            };
            Sut.Handle(accountRegistered);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.AreEqual(CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("US")).CountryISOCode.Code, dto.Settings.Country.Code);
            }
        }

        [Test]
        public void when_facebook_account_registered_then_account_dto_populated()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                FacebookId = "123456789"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Bob", dto.Name);
                Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                Assert.AreEqual("123456789", dto.FacebookId);
            }
        }

        [Test]
        public void when_twitter_account_registered_then_account_dto_populated()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                TwitterId = "123456789"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Bob", dto.Name);
                Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                Assert.AreEqual("123456789", dto.TwitterId);
            }
        }
    }

    [TestFixture]
    public class given_existing_account : given_a_view_model_generator
    {
        private Guid _accountId = Guid.NewGuid();

        public given_existing_account()
        {
            Sut.Handle(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                Password = new byte[] {1},
            });
        }

        [TestFixture]
        public class given_settings_account : given_a_view_model_generator
        {
            private readonly Guid _accountId = Guid.NewGuid();

            public given_settings_account()
            {
                Sut.Handle(new AccountRegistered
                {
                    SourceId = _accountId,
                    Name = "Bob",
                    Email = "bob.smith@acpurium.com",
                    Password = new byte[] {1}
                });
            }

            [Test]
            public void when_account_granted_admin_access_then_account_dto_populated()
            {
                Sut.Handle(new RoleAddedToUserAccount
                {
                    SourceId = _accountId,
                    RoleName = "Admin",
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);
                    Assert.AreEqual(true, dto.IsAdmin);
                }
            }

            [Test]
            public void when_settings_updated_then_account_dto_populated()
            {
                Sut.Handle(new BookingSettingsUpdated
                {
                    SourceId = _accountId,
                    Name = "Robert",
                    ChargeTypeId = 123,
                    NumberOfTaxi = 3,
                    Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                    Phone = "123",
                    Passengers = 3,
                    ProviderId = 85,
                    VehicleTypeId = 69,
                    AccountNumber = "1234",
                    CustomerNumber = "0"
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);

                    Assert.NotNull(dto);
                    Assert.AreEqual("Robert", dto.Settings.Name);
                    Assert.AreEqual(123, dto.Settings.ChargeTypeId);
                    Assert.AreEqual(3, dto.Settings.NumberOfTaxi);
                    Assert.AreEqual(CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode.Code, dto.Settings.Country.Code);
                    Assert.AreEqual("123", dto.Settings.Phone);
                    Assert.AreEqual(3, dto.Settings.Passengers);
                    Assert.AreEqual(85, dto.Settings.ProviderId);
                    Assert.AreEqual(69, dto.Settings.VehicleTypeId);
                    Assert.AreEqual("1234", dto.Settings.AccountNumber);
                    Assert.AreEqual("0", dto.Settings.CustomerNumber);
                }
            }

            public void when_settings_updated_with_null_country_code()
            {
                Sut.Handle(new BookingSettingsUpdated
                {
                    SourceId = _accountId,
                    Name = "Robert",
                    ChargeTypeId = 123,
                    NumberOfTaxi = 3,
                    Country = null,
                    Phone = "123",
                    Passengers = 3,
                    ProviderId = 85,
                    VehicleTypeId = 69,
                    AccountNumber = "1234",
                    CustomerNumber = "0"
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);

                    Assert.NotNull(dto);
                    Assert.AreEqual(CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("US")).CountryISOCode.Code, dto.Settings.Country.Code);
                }
            }

            [Test]
            public void when_settings_updated_with_default_values_then_account_dto_populated()
            {
                Sut.Handle(new BookingSettingsUpdated
                {
                    SourceId = _accountId,
                    Name = "Robert",
                    ChargeTypeId = 1,
                    NumberOfTaxi = 3,
                    Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                    Phone = "123",
                    Passengers = 3,
                    ProviderId = 1,
                    VehicleTypeId = 1,
                    AccountNumber = "1234",
                    CustomerNumber = "0"
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);

                    Assert.NotNull(dto);
                    Assert.IsNull(dto.Settings.ChargeTypeId);
                    Assert.IsNull(dto.Settings.ProviderId);
                    Assert.IsNull(dto.Settings.VehicleTypeId);
                    Assert.AreEqual(dto.Settings.Country.Code, CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode.Code);
                }
            }

            [Test]
            public void when_update_payment_profile_then_account_dto_updated()
            {
                int? defaultTipPercent = 15;

                Sut.Handle(new PaymentProfileUpdated
                {
                    SourceId = _accountId,
                    DefaultTipPercent = defaultTipPercent
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);

                    Assert.NotNull(dto);
                    Assert.AreEqual(defaultTipPercent, dto.DefaultTipPercent);
                }
            }
        }

        [Test]
        public void when_account_confirmed_then_account_dto_updated()
        {
            Sut.Handle(new AccountConfirmed
            {
                SourceId = _accountId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(true, dto.IsConfirmed);
                Assert.AreEqual(false, dto.DisabledByAdmin);
            }
        }

        [Test]
        public void when_account_disabled_then_account_dto_updated()
        {
            Sut.Handle(new AccountDisabled
            {
                SourceId = _accountId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(false, dto.IsConfirmed);
                Assert.AreEqual(true, dto.DisabledByAdmin);
            }
        }

        [Test]
        public void when_account_reset_password()
        {
            var service = new PasswordService();
            Sut.Handle(new AccountPasswordReset
            {
                SourceId = _accountId,
                Password = service.EncodePassword("Yop", _accountId.ToString())
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), dto.Password));
            }
        }

        [Test]
        public void when_account_updated_password()
        {
            var service = new PasswordService();
            Sut.Handle(new AccountPasswordUpdated
            {
                SourceId = _accountId,
                Password = service.EncodePassword("Yop", _accountId.ToString())
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), dto.Password));
            }
        }

        [Test]
        public void when_account_updated_then_account_dto_populated()
        {
            Sut.Handle(new AccountUpdated
            {
                SourceId = _accountId,
                Name = "Robert",
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Robert", dto.Name);
            }
        }

        [Test]
        public void when_account_linked_to_home_ibs_then_dto_populated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 122
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(122, dto.IBSAccountId);
            }
        }

        [Test]
        public void when_account_linked_to_roaming_ibs_then_dto_populated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 555,
                CompanyKey = "test"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Query<AccountIbsDetail>().FirstOrDefault(x => x.AccountId == _accountId && x.CompanyKey == "test");

                Assert.NotNull(dto);
                Assert.AreEqual(555, dto.IBSAccountId);
            }
        }

        [Test]
        public void when_account_unlinked_from_ibs_then_dto_updated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 122
            });

            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 555,
                CompanyKey = "test"
            });

            Sut.Handle(new AccountUnlinkedFromIbs
            {
                SourceId = _accountId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var accountDetail = context.Query<AccountDetail>().FirstOrDefault(x => x.Id == _accountId);
                var accountIbsDetail = context.Query<AccountIbsDetail>().FirstOrDefault(x => x.AccountId == _accountId);

                Assert.NotNull(accountDetail);
                Assert.AreEqual(null, accountDetail.IBSAccountId);
                Assert.Null(accountIbsDetail);
            }
        }

        [Test]
        public void when_creditcard_deactivated_then_dto_updated()
        {
            Sut.Handle(new CreditCardDeactivated { SourceId = _accountId });

            using (var context = new BookingDbContext(DbName))
            {
                var accountDetail = context.Query<AccountDetail>().FirstOrDefault(x => x.Id == _accountId);

                Assert.NotNull(accountDetail);
                Assert.AreEqual(ChargeTypes.PaymentInCar.Id, accountDetail.Settings.ChargeTypeId);
            }
        }
    }
}