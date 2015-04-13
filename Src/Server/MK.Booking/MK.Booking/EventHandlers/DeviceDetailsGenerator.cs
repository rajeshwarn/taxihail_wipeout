#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class DeviceDetailsGenerator :
        IEventHandler<DeviceRegisteredForPushNotifications>,
        IEventHandler<DeviceUnregisteredForPushNotifications>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IDeviceDao _deviceDao;

        public DeviceDetailsGenerator(Func<BookingDbContext> contextFactory, IDeviceDao deviceDao)
        {
            _contextFactory = contextFactory;
            _deviceDao = deviceDao;
        }

        public void Handle(DeviceRegisteredForPushNotifications @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var devices = context.Set<DeviceDetail>().Where(d => d.DeviceToken == @event.DeviceToken);

                context.Set<DeviceDetail>().RemoveRange(devices.Where(d=>d.AccountId != @event.SourceId));

                if ( devices.None(d=>d.AccountId == @event.SourceId ) )
                {
                    var device = new DeviceDetail
                    {
                        AccountId = @event.SourceId,
                        DeviceToken = @event.DeviceToken,
                        Platform = @event.Platform
                    };
                    context.Set<DeviceDetail>().Add(device);
                    
                }

                context.SaveChanges();
               
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