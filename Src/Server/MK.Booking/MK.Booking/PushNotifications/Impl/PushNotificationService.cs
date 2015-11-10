using System;
using System.Collections.Generic;
using System.IO;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Blackberry;
using PushSharp.Core;
using ILogger = apcurium.MK.Common.Diagnostic.ILogger;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly PushBroker _push;
        private bool _androidStarted;
        private bool _blackberryStarted;
        private bool _appleStarted;

        public PushNotificationService(IServerSettings serverSettings, ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _push = new PushBroker();

            //Wire up the events
            _push.OnDeviceSubscriptionExpired += OnDeviceSubscriptionExpired;
            _push.OnDeviceSubscriptionChanged += OnDeviceSubscriptionChanged;
            _push.OnChannelException += OnChannelException;
            _push.OnNotificationFailed += OnNotificationFailed;
            _push.OnNotificationSent += OnNotificationSent;
            _push.OnChannelCreated += OnChannelCreated;
            _push.OnChannelDestroyed += OnChannelDestroyed;
        }

        public void Send(string alert, IDictionary<string, object> data, string deviceToken, PushNotificationServicePlatform platform)
        {

            switch (platform)
            {
                case PushNotificationServicePlatform.Apple:
                    EnsureAppleStarted();
                    SendAppleNotification(alert, data, deviceToken);
                    break;
                case PushNotificationServicePlatform.Android:
                    EnsureAndroidStarted();
                    SendAndroidNotification(alert, data, deviceToken);
                    break;
                case PushNotificationServicePlatform.BlackBerry:
                    EnsureBlackberryStarted();
                    SendBBNotification(alert, data, deviceToken);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void EnsureAppleStarted()
        {
            if (_appleStarted)
            {
                return;
            }

            _appleStarted = true;

#if DEBUG
            const bool production = false;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                _serverSettings.ServerData.APNS.DevelopmentCertificatePath);
#else
            const bool production = true;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                _serverSettings.ServerData.APNS.ProductionCertificatePath);
#endif

            // Apple settings placed next for development purpose. (Crashing the method when certificate is missing.)
            var appleCert = File.ReadAllBytes(certificatePath);
            var appleSettings = new ApplePushChannelSettings(production, appleCert, _serverSettings.ServerData.APNS.CertificatePassword);
            _push.RegisterAppleService(appleSettings);
        }

        private void EnsureAndroidStarted()
        {
            if (_androidStarted)
            {
                return;
            }

            _androidStarted = true;

            var test = _serverSettings.ServerData.GCM.SenderId;
            var apiKey = _serverSettings.ServerData.GCM.APIKey;
            var androidSettings = new GcmPushChannelSettings(test, apiKey, _serverSettings.ServerData.GCM.PackageName);

            _push.RegisterGcmService(androidSettings);
        }

        private void EnsureBlackberryStarted()
        {
            if (_blackberryStarted)
            {
                return;
            }

            _blackberryStarted = true;

            var bbSettings = new BlackberryPushChannelSettings(_serverSettings.ServerData.BBNotificationSettings.AppId, _serverSettings.ServerData.BBNotificationSettings.Password);
            bbSettings.OverrideSendUrl(_serverSettings.ServerData.BBNotificationSettings.Url);

            _push.RegisterBlackberryService(bbSettings, null);
        }

        private void SendAndroidNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            _push.QueueNotification(new GcmNotification() { DelayWhileIdle = false }
              .ForDeviceRegistrationId(registrationId)
              .WithCollapseKey(Guid.NewGuid().ToString())
              .WithDelayWhileIdle(false)
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
                notification.WithCustomItem(key, new[] { data[key] });
            }

            _push.QueueNotification(notification);
        }


        private void SendBBNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            var notif = new BlackberryNotification();
            notif.Recipients.Add(new BlackberryRecipient(registrationId));
            notif.Content = new BlackberryMessageContent(JsonConvert.SerializeObject(payload));
            notif.SourceReference = _serverSettings.ServerData.BBNotificationSettings.AppId;

            _push.QueueNotification(notif);
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