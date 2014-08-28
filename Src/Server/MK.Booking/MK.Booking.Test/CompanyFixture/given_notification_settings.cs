using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using MK.Common.Configuration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_notification_settings
    {
        private EventSourcingTestHelper<Company> _sut;
        private EventSourcingTestHelper<Account> _otherSut;
        private readonly Guid _companyId = AppConstants.CompanyId;
        private readonly Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void setup()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _otherSut = new EventSourcingTestHelper<Account>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository, _otherSut.Repository));
            _sut.Given(new CompanyCreated { SourceId = _companyId });
            _otherSut.Given(new AccountRegistered { SourceId = _accountId });
        }

        [Test]
        public void when_updating_company_notification_settings()
        {
            _sut.When(new AddOrUpdateNotificationSettings
            {
                CompanyId = _companyId,
                NotificationSettings = new NotificationSettings
                {
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    NearbyTaxiPush = true,
                    DriverAssignedPush = true,
                    PaymentConfirmationPush = true,
                    ReceiptEmail = true,
                    VehicleAtPickupPush = true
                }
            });

            _otherSut.ThenHasNo<NotificationSettingsAddedOrUpdated>();
            var evt = _sut.ThenHasSingle<NotificationSettingsAddedOrUpdated>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_companyId, evt.NotificationSettings.Id);
            Assert.AreEqual(true, evt.NotificationSettings.Enabled);
            Assert.AreEqual(true, evt.NotificationSettings.BookingConfirmationEmail);
            Assert.AreEqual(true, evt.NotificationSettings.ConfirmPairingPush);
            Assert.AreEqual(true, evt.NotificationSettings.NearbyTaxiPush);
            Assert.AreEqual(true, evt.NotificationSettings.DriverAssignedPush);
            Assert.AreEqual(true, evt.NotificationSettings.PaymentConfirmationPush);
            Assert.AreEqual(true, evt.NotificationSettings.ReceiptEmail);
            Assert.AreEqual(true, evt.NotificationSettings.VehicleAtPickupPush);
        }

        [Test]
        public void when_updating_account_notification_settings()
        {
            _sut.When(new AddOrUpdateNotificationSettings
            {
                AccountId = _accountId,
                CompanyId = _companyId,
                NotificationSettings = new NotificationSettings
                {
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    NearbyTaxiPush = true,
                    DriverAssignedPush = true,
                    PaymentConfirmationPush = true,
                    ReceiptEmail = true,
                    VehicleAtPickupPush = true
                }
            });

            _sut.ThenHasNo<NotificationSettingsAddedOrUpdated>();
            var evt = _otherSut.ThenHasSingle<NotificationSettingsAddedOrUpdated>();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_accountId, evt.NotificationSettings.Id);
            Assert.AreEqual(true, evt.NotificationSettings.Enabled);
            Assert.AreEqual(true, evt.NotificationSettings.BookingConfirmationEmail);
            Assert.AreEqual(true, evt.NotificationSettings.ConfirmPairingPush);
            Assert.AreEqual(true, evt.NotificationSettings.NearbyTaxiPush);
            Assert.AreEqual(true, evt.NotificationSettings.DriverAssignedPush);
            Assert.AreEqual(true, evt.NotificationSettings.PaymentConfirmationPush);
            Assert.AreEqual(true, evt.NotificationSettings.ReceiptEmail);
            Assert.AreEqual(true, evt.NotificationSettings.VehicleAtPickupPush);
        }
    }
}