using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService: IPushNotificationService
    {
        readonly IConfigurationManager _configurationManager;
        readonly ILogger _logger;
        readonly PushService _push;
        private bool _started = false;

        public PushNotificationService(IConfigurationManager configurationManager, ILogger logger)
        {
            _configurationManager = configurationManager;
            _logger = logger;
            //Create our service	
            _push = new PushService();
        }


        public void Send(string alert, IDictionary<string, object> data, string deviceToken, PushNotificationServicePlatform platform)
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
            var production = false;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configurationManager.GetSetting("APNS.DevelopmentCertificatePath"));
#else
            var production = true;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configurationManager.GetSetting("APNS.ProductionCertificatePath"));
#endif
            // Push notifications
            
            var androidSettings = new GcmPushChannelSettings(_configurationManager.GetSetting("GCM.SenderId"), _configurationManager.GetSetting("GCM.APIKey"), _configurationManager.GetSetting("GCM.PackageName"));

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
            var appleCert = File.ReadAllBytes(certificatePath);                       
            var appleSettings = new ApplePushChannelSettings(production, appleCert, _configurationManager.GetSetting("APNS.CertificatePassword"));
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
            var notification = NotificationFactory.Apple()
                                                  .ForDeviceToken(deviceToken)
                                                  .WithAlert(alert)
                                                  .WithSound("default");
            foreach (var key in data.Keys)
            {
                notification.WithCustomItem(key, new[] {data[key]});
            }

            _push.QueueNotification(notification);
        }

        void Events_OnDeviceSubscriptionIdChanged(PlatformType platform, string oldDeviceInfo, string newDeviceInfo, Notification notification)
		{
			//Currently this event will only ever happen for Android GCM
            _logger.LogMessage("Device Registration Changed:  Old-> " + oldDeviceInfo + "  New-> " + newDeviceInfo);
		}

		void Events_OnNotificationSent(Notification notification)
		{
            _logger.LogMessage("Sent: " + notification.Platform.ToString() + " -> " + notification.ToString());
		}

		void Events_OnNotificationSendFailure(Notification notification, Exception notificationFailureException)
		{
            _logger.LogMessage("Failure: " + notification.Platform.ToString() + " -> " + notificationFailureException.Message + " -> " + notification.ToString());
		}

		void Events_OnChannelException(Exception exception, PlatformType platformType, Notification notification)
		{
            _logger.LogError(exception);
		}

		void Events_OnDeviceSubscriptionExpired(PlatformType platform, string deviceInfo, Notification notification)
		{
            _logger.LogMessage("Device Subscription Expired: " + platform.ToString() + " -> " + deviceInfo);
		}

		void Events_OnChannelDestroyed(PlatformType platformType, int newChannelCount)
		{
            _logger.LogMessage("Channel Destroyed for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}

		void Events_OnChannelCreated(PlatformType platformType, int newChannelCount)
		{
            _logger.LogMessage("Channel Created for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}
    }
}
