#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_device_detail_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected DeviceDetailsGenerator Sut;

        public given_a_device_detail_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new DeviceDetailsGenerator(() => new BookingDbContext(DbName), new DeviceDao((() => new BookingDbContext(DbName))));
        }
    }

    [TestFixture]
    public class given_no_device : given_a_device_detail_view_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_device_registered_then_device_dto_populated()
        {
            var deviceToken = Guid.NewGuid().ToString();

            Sut.Handle(new DeviceRegisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Set<DeviceDetail>().Find(_accountId, deviceToken);

                Assert.NotNull(dto);
                Assert.AreEqual(_accountId, dto.AccountId);
                Assert.AreEqual(deviceToken, dto.DeviceToken);
                Assert.AreEqual(PushNotificationServicePlatform.Android, dto.Platform);
            }
        }
    }

    [TestFixture]
    public class given_existing_device : given_a_device_detail_view_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly string _deviceToken = Guid.NewGuid().ToString();

        public given_existing_device()
        {
            Sut.Handle(new DeviceRegisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = _deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });
        }

        [Test]
        public void when_device_unregistered_then_device_dto_deleted()
        {
            Sut.Handle(new DeviceUnregisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = _deviceToken,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Set<DeviceDetail>().Find(_accountId, _deviceToken);
                Assert.Null(dto);
            }
        }

        [Test]
        public void when_device_registered_with_another_account_then_old_device_dto_deleted()
        {
            var newAccountId = Guid.NewGuid();

            Sut.Handle(new DeviceRegisteredForPushNotifications
            {
                SourceId = newAccountId,
                DeviceToken = _deviceToken
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Set<DeviceDetail>().Find(_accountId, _deviceToken);
                Assert.Null(dto);

                var otherDto = context.Set<DeviceDetail>().Find(newAccountId, _deviceToken);

                Assert.NotNull(otherDto);
                Assert.AreEqual(newAccountId, otherDto.AccountId);
                Assert.AreEqual(_deviceToken, otherDto.DeviceToken);
            }
        }
    }
}