#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
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

            Sut = new AccountDetailsGenerator(new EntityProjectionSet<AccountDetail>(() => new BookingDbContext(DbName)));
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
                Language = "fr",
                NbPassengers = 2,
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode
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
                Assert.AreEqual(2, dto.Settings.Passengers);
            }
        }

        [Test]
        public void when_account_registered_then_account_is_not_confirmed()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                NbPassengers = 1
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
                Password = new byte[] {1},
                NbPassengers = 1
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
        public void when_facebook_account_registered_then_account_dto_populated()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                Name = "Bob",
                Email = "bob.smith@acpurium.com",
                FacebookId = "123456789",
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                NbPassengers = 1
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
                TwitterId = "123456789",
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                NbPassengers = 1
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
                Country = CountryCode.GetCountryCodeByIndex(0).CountryISOCode,
                NbPassengers = 1
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
                    Password = new byte[] {1},
                    Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                    NbPassengers = 1
                });
            }

            [Test]
            public void when_account_granted_admin_access_then_account_dto_populated()
            {
                Sut.Handle(new RoleAddedToUserAccount
                {
                    SourceId = _accountId,
                    RoleName = "Support",
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var dto = context.Find<AccountDetail>(_accountId);
                    Assert.AreEqual(true, dto.HasAdminAccess);
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
                    Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                    Assert.AreEqual(123, dto.Settings.ChargeTypeId);
                    Assert.AreEqual(3, dto.Settings.NumberOfTaxi);
                    Assert.AreEqual(CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode.Code, dto.Settings.Country.Code);
                    Assert.AreEqual("123", dto.Settings.Phone);
                    Assert.AreEqual(3, dto.Settings.Passengers);
                    Assert.AreEqual(85, dto.Settings.ProviderId);
                    Assert.AreEqual(69, dto.Settings.VehicleTypeId);
                    Assert.AreEqual("1234", dto.Settings.AccountNumber);
                    Assert.AreEqual("0", dto.Settings.CustomerNumber);
                    Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                }
            }

            [Test]
            public void when_settings_updated_with_event_containing_email_then_account_dto_populated()
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
                    CustomerNumber = "0",
                    Email = "new@email.com"
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
                    Assert.AreEqual("new@email.com", dto.Email);
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
        public void when_account_linked_to_another_ibs_then_dto_not_updated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 122
            });

            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 123,
                CompanyKey = "another"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(122, dto.IBSAccountId);
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

                Assert.NotNull(accountDetail);
                Assert.AreEqual(null, accountDetail.IBSAccountId);
            }
        }

        [Test]
        public void when_creditcard_deactivated_then_dto_updated()
        {
            Sut.Handle(new CreditCardDeactivated { SourceId = _accountId, IsOutOfAppPaymentDisabled = false});

            using (var context = new BookingDbContext(DbName))
            {
                var accountDetail = context.Query<AccountDetail>().FirstOrDefault(x => x.Id == _accountId);

                Assert.NotNull(accountDetail);
                Assert.AreEqual(ChargeTypes.PaymentInCar.Id, accountDetail.Settings.ChargeTypeId);
            }
        }
    }
}