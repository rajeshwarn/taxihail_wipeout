#region
using System;
using System.Collections.Generic;
using System.IO;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;
#endregion

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;
        private readonly PushBroker _push;
        private bool _started;

        public PushNotificationService(IConfigurationManager configurationManager, ILogger logger)
        {
            _configurationManager = configurationManager;
            _logger = logger;
            _push = new PushBroker();
        }


        public void Send(string alert, IDictionary<string, object> data, string deviceToken,
            PushNotificationServicePlatform platform)
        {
            EnsureStarted();

            switch (platform)
            {
                case PushNotificationServicePlatform.Apple:
                    SendAppleNotification(alert, data, deviceToken);
                    break;
                case PushNotificationServicePlatform.Android:
                    SendAndroidNotification(alert, data, deviceToken);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void EnsureStarted()
        {
            if (_started) return;

            _started = true;

#if DEBUG
            const bool production = false;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                _configurationManager.ServerData.APNS.DevelopmentCertificatePath);
#else
            const bool production = true;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                _configurationManager.ServerData.APNS.ProductionCertificatePath);
#endif
            // Push notifications
            var test = _configurationManager.ServerData.GCM.SenderId;
            var apiKey = _configurationManager.ServerData.GCM.APIKey;
            var androidSettings = new GcmPushChannelSettings(test, apiKey, _configurationManager.ServerData.GCM.PackageName);

            //Wire up the events
            _push.OnDeviceSubscriptionExpired += OnDeviceSubscriptionExpired;
            _push.OnDeviceSubscriptionChanged += OnDeviceSubscriptionChanged;
            _push.OnChannelException += OnChannelException;
            _push.OnNotificationFailed += OnNotificationFailed;
            _push.OnNotificationSent += OnNotificationSent;
            _push.OnChannelCreated += OnChannelCreated;
            _push.OnChannelDestroyed += OnChannelDestroyed;

            _push.RegisterGcmService(androidSettings);

            // Apple settings placed next for development purpose. (Crashing the method when certificate is missing.)
            var appleCert = File.ReadAllBytes(certificatePath);
            var appleSettings = new ApplePushChannelSettings(production, appleCert, _configurationManager.ServerData.APNS.CertificatePassword);
            _push.RegisterAppleService(appleSettings);
        }

        private void SendAndroidNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            _push.QueueNotification(new GcmNotification()
                .ForDeviceRegistrationId(registrationId)
                .WithCollapseKey("NONE")
                .WithJson(JsonConvert.SerializeObject(payload)));
        }

        private void SendAppleNotification(string alert, IDictionary<string, object> data, string deviceToken)
        {
            var notification = new AppleNotification()
                .ForDeviceToken(deviceToken)
                .WithAlert(alert)
                .WithSound("default");
            foreach (var key in data.Keys)
            {
                notification.WithCustomItem(key, new[] {data[key]});
            }

            _push.QueueNotification(notification);
        }

        private void OnDeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
		{
            //Currently this event will only ever happen for Android GCM
            _logger.LogMessage("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId);
        }

        private void OnNotificationSent(object sender, INotification notification)
        {
            _logger.LogMessage("Sent: " + notification.Tag + " -> " + notification);
        }

        private void OnNotificationFailed(object sender, INotification notification, Exception notificationFailureException)
        {
            var message = notificationFailureException.Message;
            var details = notificationFailureException as NotificationFailureException;
            if (details != null)
            {
                message = details.ErrorStatusCode + " " + details.ErrorStatusDescription;
            }
            _logger.LogMessage("Failure: " + notification.Tag + " -> " + message + " -> " + notification);
        }

        private void OnChannelException(object sender, IPushChannel channel, Exception exception)
        {
            _logger.LogError(exception);
        }

        private void OnDeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
        {
            _logger.LogMessage("Device Subscription Expired: " + expiredDeviceSubscriptionId + " -> " + timestamp + " " + notification);
        }

        private void OnChannelDestroyed(object sender)
        {
            _logger.LogMessage("Channel Destroyed for: " + sender);
        }

        private void OnChannelCreated(object sender, IPushChannel pushChannel)
        {
            _logger.LogMessage("Channel Created for: " + sender);
        }
    }
}