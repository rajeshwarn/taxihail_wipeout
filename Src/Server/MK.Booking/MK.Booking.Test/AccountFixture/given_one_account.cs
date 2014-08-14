#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_one_account
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Account>();

            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService(), null));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com",
                IbsAcccountId = 10,
                ConfirmationToken = _confimationToken
            });
        }

        private EventSourcingTestHelper<Account> _sut;
        private Guid _accountId = Guid.NewGuid();
        private readonly string _confimationToken = Guid.NewGuid().ToString();

        [Test]
        public void when_adding_an_address_successfully()
        {
            var addressId = Guid.NewGuid();
            _sut.When(new AddFavoriteAddress
            {
                AccountId = _accountId,
                Address = new Address
                {
                    Id = addressId,
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });

            var evt = _sut.ThenHasSingle<FavoriteAddressAdded>();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(addressId, evt.Address.Id);
            Assert.AreEqual("Chez François", evt.Address.FriendlyName);
            Assert.AreEqual("3939", evt.Address.Apartment);
            Assert.AreEqual("1234 rue Saint-Hubert", evt.Address.FullAddress);
            Assert.AreEqual("3131", evt.Address.RingCode);
            Assert.AreEqual("Hôtel de Ville", evt.Address.BuildingName);
            Assert.AreEqual(45.515065, evt.Address.Latitude);
            Assert.AreEqual(-73.558064, evt.Address.Longitude);
        }

        [Test]
        public void when_adding_an_address_with_and_invalid_latitude_or_longitude()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.When(new AddFavoriteAddress
            {
                AccountId = _accountId,
                Address = new Address
                {
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 180,
                    Longitude = -73.558064
                }
            }));

            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.When(new AddFavoriteAddress
            {
                AccountId = _accountId,
                Address = new Address
                {
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 0,
                    Longitude = -200.558064
                }
            }));
        }

        [Test]
        public void when_adding_an_address_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new AddFavoriteAddress
            {
                AccountId = _accountId,
                Address =
                    new Address
                    {
                        FriendlyName = null,
                        Apartment = "3939",
                        FullAddress = null,
                        RingCode = "3131",
                        Latitude = 45.515065,
                        Longitude = -73.558064
                    }
            }));
        }

        [Test]
        public void when_account_ccs_deleted()
        {
            _sut.When(new DeleteAccountCreditCards
            {
                AccountId = _accountId
            });

            var @event = _sut.ThenHasSingle<AllCreditCardsRemoved>();

            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_all_ccs_deleted()
        {
            var accounts = new[] {_accountId};
            _sut.When(new DeleteAllCreditCards
            {
                AccountIds = accounts
            });

            var @event = _sut.ThenHasSingle<AllCreditCardsRemoved>();

            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_confirming_account_successfully()
        {
            _sut.When(new ConfirmAccount {AccountId = _accountId, ConfimationToken = _confimationToken});

            var @event = _sut.ThenHasSingle<AccountConfirmed>();
            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_confirming_account_with_invalid_token()
        {
            Assert.Throws<InvalidOperationException>(
                () => _sut.When(new ConfirmAccount {AccountId = _accountId, ConfimationToken = "invalid"}),
                "Invalid confirmation token");
        }

        [Test]
        public void when_confirming_byadmin_account_successfully()
        {
            _sut.When(new EnableAccountByAdmin {AccountId = _accountId});

            var @event = _sut.ThenHasSingle<AccountConfirmed>();
            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_granting_admin_rights_successfully()
        {
            _sut.When(new AddRoleToUserAccount {AccountId = _accountId, RoleName = "Admin"});

            var @event = _sut.ThenHasSingle<RoleAddedToUserAccount>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Admin", @event.RoleName);
        }

        [Test]
        public void when_registering_device_sucessfully()
        {
            var deviceToken = Guid.NewGuid().ToString();
            _sut.When(new RegisterDeviceForPushNotifications
            {
                AccountId = _accountId,
                DeviceToken = deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

            var @event = _sut.ThenHasSingle<DeviceRegisteredForPushNotifications>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(deviceToken, @event.DeviceToken);
            Assert.AreEqual(PushNotificationServicePlatform.Android, @event.Platform);
        }

        [Test]
        public void when_removing_address_from_history_successfully()
        {
            var addressId = Guid.NewGuid();
            _sut.When(new RemoveAddressFromHistory {AddressId = addressId, AccountId = _accountId});

            var @event = _sut.ThenHasSingle<AddressRemovedFromHistory>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(addressId, @event.AddressId);
        }

        [Test]
        public void when_replacing_device_sucessfully()
        {
            var deviceToken = Guid.NewGuid().ToString();
            var oldDeviceToken = Guid.NewGuid().ToString();
            _sut.When(new RegisterDeviceForPushNotifications
            {
                AccountId = _accountId,
                DeviceToken = deviceToken,
                OldDeviceToken = oldDeviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

            var event1 = _sut.ThenHasOne<DeviceUnregisteredForPushNotifications>();
            var event2 = _sut.ThenHasOne<DeviceRegisteredForPushNotifications>();

            Assert.AreEqual(_accountId, event1.SourceId);
            Assert.AreEqual(oldDeviceToken, event1.DeviceToken);
            Assert.AreEqual(_accountId, event2.SourceId);
            Assert.AreEqual(deviceToken, event2.DeviceToken);
            Assert.AreEqual(PushNotificationServicePlatform.Android, event2.Platform);
        }

        [Test]
        public void when_reseting_password_successfully()
        {
            _sut.When(new ResetAccountPassword {AccountId = _accountId, Password = "Yop"});

            var @event = _sut.ThenHasSingle<AccountPasswordReset>();

            Assert.AreEqual(_accountId, @event.SourceId);

            var service = new PasswordService();

            Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), @event.Password));
        }

        [Test]
        public void when_unregistering_device_sucessfully()
        {
            var deviceToken = Guid.NewGuid().ToString();
            _sut.When(new UnregisterDeviceForPushNotifications
            {
                AccountId = _accountId,
                DeviceToken = deviceToken,
            });

            var @event = _sut.ThenHasSingle<DeviceUnregisteredForPushNotifications>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(deviceToken, @event.DeviceToken);
        }

        [Test]
        public void when_updating_password_successfully()
        {
            _sut.When(new UpdateAccountPassword {AccountId = _accountId, Password = "Yop"});

            var @event = _sut.ThenHasSingle<AccountPasswordUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);

            var service = new PasswordService();

            Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), @event.Password));
        }

        [Test]
        public void when_updating_successfully()
        {
            _sut.When(new UpdateAccount {AccountId = _accountId, Name = "Robert"});

            var @event = _sut.ThenHasSingle<AccountUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Robert", @event.Name);
        }
    }
}