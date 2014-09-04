using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;
using MK.Common.Configuration;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    public class given_a_notification_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected NotificationSettingsGenerator Sut;

        public given_a_notification_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new NotificationSettingsGenerator(() => new ConfigurationDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_notification_settings_for_account : given_a_notification_model_generator
    {
        [Test]
        public void when_settings_dont_exist_and_settings_updated_then_dto_populated()
        {
            var accountId = Guid.NewGuid();

            Sut.Handle(new NotificationSettingsAddedOrUpdated
            {
                SourceId = accountId,
                NotificationSettings = new NotificationSettings
                {
                    Id = accountId,
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    DriverAssignedPush = true,
                    NearbyTaxiPush = true,
                    PaymentConfirmationPush = true,
                    ReceiptEmail = true,
                    VehicleAtPickupPush = true
                }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.NotificationSettings.Find(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(accountId, dto.Id);
                Assert.AreEqual(true, dto.Enabled);
                Assert.AreEqual(true, dto.BookingConfirmationEmail);
                Assert.AreEqual(true, dto.ConfirmPairingPush);
                Assert.AreEqual(true, dto.DriverAssignedPush);
                Assert.AreEqual(true, dto.NearbyTaxiPush);
                Assert.AreEqual(true, dto.ReceiptEmail);
                Assert.AreEqual(true, dto.VehicleAtPickupPush);
            }
        }
    }

    [TestFixture]
    public class given_a_notification_settings_for_account : given_a_notification_model_generator
    {
        private Guid _accountId = Guid.NewGuid();

        public given_a_notification_settings_for_account()
        {
            Sut.Handle(new NotificationSettingsAddedOrUpdated
            {
                SourceId = _accountId,
                NotificationSettings = new NotificationSettings
                {
                    Id = _accountId,
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    DriverAssignedPush = true,
                    NearbyTaxiPush = true,
                    PaymentConfirmationPush = true,
                    ReceiptEmail = true,
                    VehicleAtPickupPush = true
                }
            });
        }

        [Test]
        public void when_settings_exist_and_settings_updated_then_dto_updated()
        {
            Sut.Handle(new NotificationSettingsAddedOrUpdated
            {
                SourceId = _accountId,
                NotificationSettings = new NotificationSettings
                {
                    Id = _accountId,
                    Enabled = false,
                    BookingConfirmationEmail = false,
                    ConfirmPairingPush = false,
                    DriverAssignedPush = false,
                    NearbyTaxiPush = false,
                    PaymentConfirmationPush = false,
                    ReceiptEmail = false,
                    VehicleAtPickupPush = false
                }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.NotificationSettings.Find(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(_accountId, dto.Id);
                Assert.AreEqual(false, dto.Enabled);
                Assert.AreEqual(false, dto.BookingConfirmationEmail);
                Assert.AreEqual(false, dto.ConfirmPairingPush);
                Assert.AreEqual(false, dto.DriverAssignedPush);
                Assert.AreEqual(false, dto.NearbyTaxiPush);
                Assert.AreEqual(false, dto.ReceiptEmail);
                Assert.AreEqual(false, dto.VehicleAtPickupPush);
            }
        }
    }
}