using System;
using System.IO;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService: IPushNotificationService
    {
        readonly ApplePushChannelSettings _appleSettings;
        readonly GcmPushChannelSettings _androidSettings;
        readonly ILogger _logger;
        readonly PushService _push;

        public PushNotificationService(ApplePushChannelSettings appleSettings, GcmPushChannelSettings androidSettings, ILogger logger)
        {
            _appleSettings = appleSettings;
            _androidSettings = androidSettings;
            _logger = logger;
            //Create our service	
            _push = new PushService();

            //Wire up the events
            _push.Events.OnDeviceSubscriptionExpired += Events_OnDeviceSubscriptionExpired;
            _push.Events.OnDeviceSubscriptionIdChanged += Events_OnDeviceSubscriptionIdChanged;
            _push.Events.OnChannelException += Events_OnChannelException;
            _push.Events.OnNotificationSendFailure += Events_OnNotificationSendFailure;
            _push.Events.OnNotificationSent += Events_OnNotificationSent;
            _push.Events.OnChannelCreated += Events_OnChannelCreated;
            _push.Events.OnChannelDestroyed += Events_OnChannelDestroyed;

            _push.StartApplePushService(_appleSettings);
            _push.StartGoogleCloudMessagingPushService(_androidSettings);
        }


        public void Send(string alert, string deviceToken, PushNotificationServicePlatform platform)
        {
            switch (platform)
            {
                case PushNotificationServicePlatform.Apple:
                    SendAppleNotification(alert, deviceToken);
                    break;
                case PushNotificationServicePlatform.Android:
                    SendAndroidNotification(alert, deviceToken);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SendAndroidNotification(string alert, string registrationId)
        {
            var payload = JsonConvert.SerializeObject(new { alert });

            _push.QueueNotification(NotificationFactory.AndroidGcm()
                .ForDeviceRegistrationId(registrationId)
                .WithCollapseKey("NONE")
                .WithJson(payload));
        }

        private void SendAppleNotification(string alert, string deviceToken)
        {
            _push.QueueNotification(NotificationFactory.Apple()
                .ForDeviceToken(deviceToken)
                .WithAlert(alert)
                .WithSound("default"));
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
