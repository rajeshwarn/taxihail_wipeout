using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender 
        : IIntegrationEventHandler,
          IEventHandler<OrderStatusChanged>
    {
        readonly Func<BookingDbContext> _contextFactory;
        readonly IPushNotificationService _pushNotificationService;

        public PushNotificationSender(Func<BookingDbContext> contextFactory, IPushNotificationService pushNotificationService)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                var data = new Dictionary<string, object>
                {
                    { "orderId", order.Id },
                };

                foreach (var device in devices)
                {
                    _pushNotificationService.Send("status changed to " + @event.Status.IBSStatusId, data, device.DeviceToken, device.Platform);
                }

            }
        }
    }
}
