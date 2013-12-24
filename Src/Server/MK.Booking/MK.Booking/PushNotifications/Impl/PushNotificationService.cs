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
using PushSharp.Common;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;
        private readonly PushService _push;
        private bool _started;

        public PushNotificationService(IConfigurationManager configurationManager, ILogger logger)
        {
            _configurationManager = configurationManager;
            _logger = logger;
            //Create our service	
            _push = new PushService();
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
            bool production = false;
            string certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                _configurationManager.GetSetting("APNS.DevelopmentCertificatePath"));
#else
            var production = true;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configurationManager.GetSetting("APNS.ProductionCertificatePath"));
#endif
            // Push notifications

            string test = _configurationManager.GetSetting("GCM.SenderId");
            string apiKey = _configurationManager.GetSetting("GCM.APIKey");
            var androidSettings = new GcmPushChannelSettings(test, apiKey,
                _configurationManager.GetSetting("GCM.PackageName"));

            //Wire up the events
            _push.Events.OnDeviceSubscriptionExpired += Events_OnDeviceSubscriptionExpired;
            _push.Events.OnDeviceSubscriptionIdChanged += Events_OnDeviceSubscriptionIdChanged;
            _push.Events.OnChannelException += Events_OnChannelException;
            _push.Events.OnNotificationSendFailure += Events_OnNotificationSendFailure;
            _push.Events.OnNotificationSent += Events_OnNotificationSent;
            _push.Events.OnChannelCreated += Events_OnChannelCreated;
            _push.Events.OnChannelDestroyed += Events_OnChannelDestroyed;

            _push.StartGoogleCloudMessagingPushService(androidSettings);

            // Apple settings placed next for development purpose. (Crashing the method when certificate is missing.)
            byte[] appleCert = File.ReadAllBytes(certificatePath);
            var appleSettings = new ApplePushChannelSettings(production, appleCert,
                _configurationManager.GetSetting("APNS.CertificatePassword"));
            _push.StartApplePushService(appleSettings);
        }

        private void SendAndroidNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            _push.QueueNotification(NotificationFactory.AndroidGcm()
                .ForDeviceRegistrationId(registrationId)
                .WithCollapseKey("NONE")
                .WithJson(JsonConvert.SerializeObject(payload)));
        }

        private void SendAppleNotification(string alert, IDictionary<string, object> data, string deviceToken)
        {
            AppleNotification notification = NotificationFactory.Apple()
                .ForDeviceToken(deviceToken)
                .WithAlert(alert)
                .WithSound("default");
            foreach (string key in data.Keys)
            {
                notification.WithCustomItem(key, new[] {data[key]});
            }

            _push.QueueNotification(notification);
        }

        private void Events_OnDeviceSubscriptionIdChanged(PlatformType platform, string oldDeviceInfo,
            string newDeviceInfo, Notification notification)
        {
            //Currently this event will only ever happen for Android GCM
            _logger.LogMessage("Device Registration Changed:  Old-> " + oldDeviceInfo + "  New-> " + newDeviceInfo);
        }

        private void Events_OnNotificationSent(Notification notification)
        {
            _logger.LogMessage("Sent: " + notification.Platform + " -> " + notification);
        }

        private void Events_OnNotificationSendFailure(Notification notification, Exception notificationFailureException)
        {
            string message = notificationFailureException.Message;
            var details = notificationFailureException as NotificationFailureException;
            if (details != null)
            {
                message = details.ErrorStatusCode + " " + details.ErrorStatusDescription;
            }
            _logger.LogMessage("Failure: " + notification.Platform + " -> " + message + " -> " + notification);
        }

        private void Events_OnChannelException(Exception exception, PlatformType platformType, Notification notification)
        {
            _logger.LogError(exception);
        }

        private void Events_OnDeviceSubscriptionExpired(PlatformType platform, string deviceInfo,
            Notification notification)
        {
            _logger.LogMessage("Device Subscription Expired: " + platform + " -> " + deviceInfo);
        }

        private void Events_OnChannelDestroyed(PlatformType platformType, int newChannelCount)
        {
            _logger.LogMessage("Channel Destroyed for: " + platformType + " Channel Count: " + newChannelCount);
        }

        private void Events_OnChannelCreated(PlatformType platformType, int newChannelCount)
        {
            _logger.LogMessage("Channel Created for: " + platformType + " Channel Count: " + newChannelCount);
        }
    }
}