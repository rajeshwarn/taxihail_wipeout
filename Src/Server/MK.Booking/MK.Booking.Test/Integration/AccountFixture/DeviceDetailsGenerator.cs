using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    public class given_a_device_detail_view_model_generator : given_a_read_model_database
    {
        protected DeviceDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_device_detail_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new DeviceDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_device : given_a_device_detail_view_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();
        public given_no_device()
        {

        }

        [Test]
        public void when_device_registered_then_device_dto_populated()
        {
            var deviceToken = Guid.NewGuid().ToString();

            this.sut.Handle(new DeviceRegisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Set<DeviceDetail>().Find(_accountId, deviceToken);

                Assert.NotNull(dto);
                Assert.AreEqual(_accountId, dto.AccountId);
                Assert.AreEqual(deviceToken, dto.DeviceToken);
                Assert.AreEqual(PushNotificationServicePlatform.Android, (PushNotificationServicePlatform)dto.Platform);
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
            this.sut.Handle(new DeviceRegisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = _deviceToken,
                Platform = PushNotificationServicePlatform.Android
            });

        }

        [Test]
        public void when_device_unregistered_then_device_dto_deleted()
        {
            this.sut.Handle(new DeviceUnregisteredForPushNotifications
            {
                SourceId = _accountId,
                DeviceToken = _deviceToken,
            });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Set<DeviceDetail>().Find(_accountId, _deviceToken);

                Assert.Null(dto);
            }
        }
    }
}
