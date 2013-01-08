using System;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_one_account
    {

        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();
        private string confimationToken = Guid.NewGuid().ToString();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();

            this.sut.Setup(new AccountCommandHandler(this.sut.Repository, new PasswordService()));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com", IbsAcccountId=10, ConfirmationToken = confimationToken});
        }

        [Test]
        public void when_confirming_account_successfully()
        {
            this.sut.When(new ConfirmAccount { AccountId = _accountId, ConfimationToken = confimationToken });

            var @event = sut.ThenHasSingle<AccountConfirmed>();
            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_confirming_account_with_invalid_token()
        {
            Assert.Throws<InvalidOperationException>(
                () => this.sut.When(new ConfirmAccount { AccountId = _accountId, ConfimationToken = "invalid" }), "Invalid confirmation token");
        }

        [Test]
        public void when_updating_successfully()
        {
            this.sut.When(new UpdateAccount { AccountId = _accountId, Name = "Robert"});

            var @event = sut.ThenHasSingle<AccountUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Robert", @event.Name);            
            
        }

        [Test]
        public void when_reseting_password_successfully()
        {
            this.sut.When(new ResetAccountPassword { AccountId = _accountId, Password = "Yop" });

            var @event = sut.ThenHasSingle<AccountPasswordReset>();

            Assert.AreEqual(_accountId, @event.SourceId);

            var service = new PasswordService();

            Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), @event.Password));

        }

        [Test]
        public void when_updating_password_successfully()
        {
            this.sut.When(new UpdateAccountPassword { AccountId = _accountId, Password = "Yop" });

            var @event = sut.ThenHasSingle<AccountPasswordUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);

            var service = new PasswordService();

            Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), @event.Password));
        }

        [Test]
        public void when_granting_admin_rights_successfully()
        {
            this.sut.When(new GrantAdminRight() { AccountId = _accountId });

            var @event = sut.ThenHasSingle<AdminRightGranted>();

            Assert.AreEqual(_accountId, @event.SourceId);

        }

        [Test]
        public void when_registering_device_sucessfully()
        {
            var deviceToken = Guid.NewGuid().ToString();
            this.sut.When(new RegisterDeviceForPushNotifications
            {
                AccountId = _accountId,
                DeviceToken = deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

            var @event = sut.ThenHasSingle<DeviceRegisteredForPushNotifications>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(deviceToken, @event.DeviceToken);
            Assert.AreEqual(PushNotificationServicePlatform.Android, @event.Platform);

        }
    }
}
