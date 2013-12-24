#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender
        : IIntegrationEventHandler,
            IEventHandler<OrderStatusChanged>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly dynamic _resources;

        public PushNotificationSender(Func<BookingDbContext> contextFactory,
            IPushNotificationService pushNotificationService, IConfigurationManager configurationManager)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;

            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new DynamicResources(applicationKey);
        }

        public void Handle(OrderStatusChanged @event)
        {
            var shouldSendPushNotification = @event.Status.IbsStatusId == VehicleStatuses.Common.Assigned ||
                                             @event.Status.IbsStatusId == VehicleStatuses.Common.Arrived ||
                                             @event.Status.IbsStatusId == VehicleStatuses.Common.Timeout;

            if (shouldSendPushNotification)
            {
                string alert;
                switch (@event.Status.IbsStatusId)
                {
                    case VehicleStatuses.Common.Assigned:
                        alert = string.Format((string) _resources.PushNotification_wosASSIGNED,
                            @event.Status.VehicleNumber);
                        break;
                    case VehicleStatuses.Common.Arrived:
                        alert = string.Format((string) _resources.PushNotification_wosARRIVED,
                            @event.Status.VehicleNumber);
                        break;
                    case VehicleStatuses.Common.Timeout:
                        alert = (string) _resources.PushNotification_wosTIMEOUT;
                        break;
                    default:
                        throw new InvalidOperationException("No push notification for this order status");
                }

                using (var context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);
                    var devices =
                        context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                    var data = new Dictionary<string, object>();
                    if (@event.Status.IbsStatusId == VehicleStatuses.Common.Assigned ||
                        @event.Status.IbsStatusId == VehicleStatuses.Common.Arrived)
                    {
                        data.Add("orderId", order.Id);
                    }

                    foreach (var device in devices)
                    {
                        _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                    }
                }
            }
        }
    }
}