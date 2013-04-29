using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.EventHandlers
{
    public class DeviceDetailsGenerator : 
        IEventHandler<DeviceRegisteredForPushNotifications>,
        IEventHandler<DeviceUnregisteredForPushNotifications>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private IConfigurationManager _configurationManager;
        public DeviceDetailsGenerator(Func<BookingDbContext> contextFactory, IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _contextFactory = contextFactory;
        }

        public void Handle(DeviceRegisteredForPushNotifications @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var device = context.Set<DeviceDetail>().Find(@event.SourceId, @event.DeviceToken);
                if (device == null)
                {
                    device = new DeviceDetail
                    {
                        AccountId = @event.SourceId,
                        DeviceToken = @event.DeviceToken,
                        Platform = @event.Platform
                    };

                    context.Save(device);
                }
            }
        }

        public void Handle(DeviceUnregisteredForPushNotifications @event)
        {
            using (var context = _contextFactory.Invoke())
            {

                var device = context.Set<DeviceDetail>().Find(@event.SourceId, @event.DeviceToken);
                if (device != null)
                {
                    context.Set<DeviceDetail>().Remove(device);
                    context.SaveChanges();
                }
            }
        }
    }
}
