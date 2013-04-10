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
        private const string wosASSIGNED = "Taxi #{0} has been assigned to you";
        private const string wosARRIVED = "Your Taxi (#{0}) has arrived";
        readonly Func<BookingDbContext> _contextFactory;
        readonly IPushNotificationService _pushNotificationService;

        public PushNotificationSender(Func<BookingDbContext> contextFactory, IPushNotificationService pushNotificationService)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
        }

        public void Handle(OrderStatusChanged @event)
        {
            var shouldSendPushNotification = @event.Status.IBSStatusId == "wosASSIGNED"
                                                || @event.Status.IBSStatusId == "wosARRIVED";

            if (shouldSendPushNotification)
            {
                var alert = string.Empty;
                switch (@event.Status.IBSStatusId)
                {
                    case "wosASSIGNED":
                        alert = string.Format(wosASSIGNED, @event.Status.VehicleNumber);
                        break;
                    case "wosARRIVED":
                        alert = string.Format(wosARRIVED, @event.Status.VehicleNumber);
                        break;
                    default:
                        throw new InvalidOperationException("No push notification for this order status");
                }

                using (var context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);
                    var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                    var data = new Dictionary<string, object>
                                   {
                                       {"orderId", order.Id},
                                   };

                    foreach (var device in devices)
                    {
                        _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                    }
                }

            }
        }
    }
}
