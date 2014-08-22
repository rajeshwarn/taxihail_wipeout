using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IAppSettings _appSettings;
        private readonly Resources.Resources _resources;

        public NotificationService(
            Func<BookingDbContext> contextFactory, 
            IPushNotificationService pushNotificationService,
            IConfigurationManager configurationManager, 
            IAppSettings appSettings)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _appSettings = appSettings;

            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void SendStatusChangedNotification(OrderStatusDetail orderStatusDetail)
        {
            var shouldSendPushNotification = orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned ||
                                             orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived ||
                                             (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded && _appSettings.Data.AutomaticPayment) ||
                                             orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Timeout;

            if (shouldSendPushNotification)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(orderStatusDetail.OrderId);

                    string alert;
                    switch (orderStatusDetail.IBSStatusId)
                    {
                        case VehicleStatuses.Common.Assigned:
                            alert = string.Format(_resources.Get("PushNotification_wosASSIGNED", order.ClientLanguageCode),
                                orderStatusDetail.VehicleNumber);
                            break;
                        case VehicleStatuses.Common.Arrived:
                            alert = string.Format(_resources.Get("PushNotification_wosARRIVED", order.ClientLanguageCode),
                                orderStatusDetail.VehicleNumber);
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

                    if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned ||
                        orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived)
                    {
                        data.Add("orderId", order.Id);
                        data.Add("isPairingNotification", false);
                    }
                    if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)
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

        public void SendPaymentCaptureNotification(Guid orderId, decimal amount)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);

                var alert = string.Format(string.Format(_resources.Get("PushNotification_PaymentReceived"), amount), order.ClientLanguageCode);
                var data = new Dictionary<string, object> { { "orderId", order.Id } };
                var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);

                // Send push notifications
                foreach (var device in devices)
                {
                    _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                }
            }
        }

        private const int TaxiDistanceThresholdForPushNotification = 200; // In meters
        public void SendTaxiNearbyNotification(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);
                var orderStatus = context.Query<OrderStatusDetail>().Single(x => x.OrderId == orderId);

                var shouldSendPushNotification = newLatitude.HasValue &&
                                                 newLongitude.HasValue &&
                                                 ibsStatus == VehicleStatuses.Common.Assigned &&
                                                 !orderStatus.IsTaxiNearbyNotificationSent;

                if (shouldSendPushNotification)
                {
                    var taxiPosition = new Position(newLatitude.Value, newLongitude.Value);
                    var pickupPosition = new Position(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                    if (taxiPosition.DistanceTo(pickupPosition) <= TaxiDistanceThresholdForPushNotification)
                    {
                        orderStatus.IsTaxiNearbyNotificationSent = true;
                        context.Save(orderStatus);

                        var alert = string.Format(_resources.Get("PushNotification_NearbyTaxi", order.ClientLanguageCode));
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
    }
}