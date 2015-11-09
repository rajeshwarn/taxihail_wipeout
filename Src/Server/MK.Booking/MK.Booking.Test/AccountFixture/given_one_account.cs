#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;
using ServiceStack.Common.Extensions;

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

            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService(),null, new TestServerSettings()));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com",
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
            _sut.When(new DeleteAccountCreditCard
            {
                AccountId = _accountId
            });

            var @event = _sut.ThenHasSingle<CreditCardRemoved>();

            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_all_ccs_deleted()
        {
            var accounts = new[] {_accountId};
            _sut.When(new DeleteCreditCardsFromAccounts
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
            _sut.When(new UpdateRoleToUserAccount {AccountId = _accountId, RoleName = "Admin"});

            var @event = _sut.ThenHasSingle<RoleUpdatedToUserAccount>();

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

        [Test]
        public void when_linking_account_to_home_ibs()
        {
            _sut.When(new LinkAccountToIbs { AccountId = _accountId, IbsAccountId = 123 });

            var @event = _sut.ThenHasSingle<AccountLinkedToIbs>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(123, @event.IbsAccountId);
        }

        [Test]
        public void when_linking_account_to_roaming_ibs()
        {
            _sut.When(new LinkAccountToIbs { AccountId = _accountId, IbsAccountId = 123, CompanyKey = "test"});

            var @event = _sut.ThenHasSingle<AccountLinkedToIbs>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(123, @event.IbsAccountId);
            Assert.AreEqual("test", @event.CompanyKey);
        }

        [Test]
        public void when_unlinking_account_from_ibs()
        {
            _sut.Given(new AccountLinkedToIbs { SourceId = _accountId, IbsAccountId = 123 });
            _sut.Given(new AccountLinkedToIbs { SourceId = _accountId, IbsAccountId = 123, CompanyKey = "test" });
            _sut.When(new UnlinkAccountFromIbs { AccountId = _accountId });

            var @event = _sut.ThenHasSingle<AccountUnlinkedFromIbs>();

            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_logging_overdue_payment()
        {
            var orderId = Guid.NewGuid();
            var transactionDate = DateTime.UtcNow;

            _sut.When(new ReactToPaymentFailure
            {
                AccountId = _accountId,
                OrderId = orderId,
                IBSOrderId = 5544,
                OverdueAmount = 42.25m,
                TransactionDate = transactionDate,
                TransactionId = "1337",
                FeeType = FeeTypes.Booking
            });

            var @event1 = (OverduePaymentLogged)_sut.ThenHas<OverduePaymentLogged>().First();
            var @event2 = (CreditCardDeactivated)_sut.ThenHas<CreditCardDeactivated>().First();

            Assert.AreEqual(_accountId, @event1.SourceId);
            Assert.AreEqual(orderId, @event1.OrderId);
            Assert.AreEqual(5544, @event1.IBSOrderId);
            Assert.AreEqual(42.25m, @event1.Amount);
            Assert.AreEqual("1337", @event1.TransactionId);
            Assert.AreEqual(transactionDate, @event1.TransactionDate);
            Assert.AreEqual(FeeTypes.Booking, @event1.FeeType);

            Assert.AreEqual(_accountId, @event2.SourceId);
        }

        [Test]
        public void when_settling_overdue_payment()
        {
            var orderId = Guid.NewGuid();

            _sut.When(new SettleOverduePayment { AccountId = _accountId, OrderId = orderId });

            var @event = _sut.ThenHasSingle<OverduePaymentSettled>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
        }
    }
}