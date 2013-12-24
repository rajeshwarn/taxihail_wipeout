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

            string applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new DynamicResources(applicationKey);
        }

        public void Handle(OrderStatusChanged @event)
        {
            bool shouldSendPushNotification = @event.Status.IBSStatusId == VehicleStatuses.Common.Assigned ||
                                              @event.Status.IBSStatusId == VehicleStatuses.Common.Arrived ||
                                              @event.Status.IBSStatusId == VehicleStatuses.Common.Timeout;

            if (shouldSendPushNotification)
            {
                string alert = string.Empty;
                switch (@event.Status.IBSStatusId)
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

                using (BookingDbContext context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);
                    IQueryable<DeviceDetail> devices =
                        context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                    var data = new Dictionary<string, object>();
                    if (@event.Status.IBSStatusId == VehicleStatuses.Common.Assigned ||
                        @event.Status.IBSStatusId == VehicleStatuses.Common.Arrived)
                    {
                        data.Add("orderId", order.Id);
                    }

                    foreach (DeviceDetail device in devices)
                    {
                        _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                    }
                }
            }
        }
    }
}