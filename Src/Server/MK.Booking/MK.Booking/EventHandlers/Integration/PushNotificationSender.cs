#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender
        : IIntegrationEventHandler,
            IEventHandler<OrderStatusChanged>,
            IEventHandler<CreditCardPaymentCaptured>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IAppSettings _appSettings;
        private readonly Resources.Resources _resources;

        public PushNotificationSender(Func<BookingDbContext> contextFactory, IPushNotificationService pushNotificationService,
                                      IConfigurationManager configurationManager, IAppSettings appSettings)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _appSettings = appSettings;

            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void Handle(OrderStatusChanged @event)
        {
            var shouldSendPushNotification = @event.Status.IBSStatusId == VehicleStatuses.Common.Assigned ||
                                             @event.Status.IBSStatusId == VehicleStatuses.Common.Arrived ||
                                             (@event.Status.IBSStatusId == VehicleStatuses.Common.Loaded && _appSettings.Data.AutomaticPayment) ||
                                             @event.Status.IBSStatusId == VehicleStatuses.Common.Timeout;

            if (shouldSendPushNotification)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);

                    string alert;
                    switch (@event.Status.IBSStatusId)
                    {
                        case VehicleStatuses.Common.Assigned:
                            alert = string.Format(_resources.Get("PushNotification_wosASSIGNED", order.ClientLanguageCode),
                                @event.Status.VehicleNumber);
                            break;
                        case VehicleStatuses.Common.Arrived:
                            alert = string.Format(_resources.Get("PushNotification_wosARRIVED", order.ClientLanguageCode),
                                @event.Status.VehicleNumber);
                            break;
                        case VehicleStatuses.Common.Loaded:
                            if (order.Settings.ChargeTypeId != ChargeTypes.CardOnFile.Id)
                            {
                                // Only send notification if card on file
                                return;
                            }
                            alert = _resources.Get("PushNotification_wosLOADED", order.ClientLanguageCode);
                            break;
                        case VehicleStatuses.Common.Timeout:
                            alert = _resources.Get("PushNotification_wosTIMEOUT", order.ClientLanguageCode);
                            break;
                        default:
                            throw new InvalidOperationException("No push notification for this order status");
                    }

                    var devices =
                        context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                    var data = new Dictionary<string, object>();

                    if (@event.Status.IBSStatusId == VehicleStatuses.Common.Assigned ||
                        @event.Status.IBSStatusId == VehicleStatuses.Common.Arrived)
                    {
                        data.Add("orderId", order.Id);
                        data.Add("isPairingNotification", false);
                    }
                    if (@event.Status.IBSStatusId == VehicleStatuses.Common.Loaded)
                    {
                        data.Add("orderId", order.Id);
                        data.Add("isPairingNotification", true);
                    }

                    foreach (var device in devices)
                    {
                        _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                    }
                }
            }
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);

                var alert =
                    string.Format(string.Format(_resources.Get("PushNotification_PaymentReceived"), @event.Amount),
                        order.ClientLanguageCode);
                var data = new Dictionary<string, object> { { "orderId", order.Id } };
                var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);

                // Send push notifications
                foreach (var device in devices)
                {
                    _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                }
            }
        }
    }
}