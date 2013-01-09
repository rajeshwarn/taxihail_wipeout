using System;
using System.IO;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Common;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService: IPushNotificationService
    {
        readonly ApplePushChannelSettings _appleSettings;
        readonly GcmPushChannelSettings _androidSettings;
        readonly PushService _push;

        public PushNotificationService(ApplePushChannelSettings appleSettings, GcmPushChannelSettings androidSettings)
        {
            _appleSettings = appleSettings;
            _androidSettings = androidSettings;
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

        static void Events_OnDeviceSubscriptionIdChanged(PlatformType platform, string oldDeviceInfo, string newDeviceInfo, Notification notification)
		{
			//Currently this event will only ever happen for Android GCM
			Console.WriteLine("Device Registration Changed:  Old-> " + oldDeviceInfo + "  New-> " + newDeviceInfo);
		}

		static void Events_OnNotificationSent(Notification notification)
		{
			Console.WriteLine("Sent: " + notification.Platform.ToString() + " -> " + notification.ToString());
		}

		static void Events_OnNotificationSendFailure(Notification notification, Exception notificationFailureException)
		{
			Console.WriteLine("Failure: " + notification.Platform.ToString() + " -> " + notificationFailureException.Message + " -> " + notification.ToString());
		}

		static void Events_OnChannelException(Exception exception, PlatformType platformType, Notification notification)
		{
			Console.WriteLine("Channel Exception: " + platformType.ToString() + " -> " + exception.ToString());
		}

		static void Events_OnDeviceSubscriptionExpired(PlatformType platform, string deviceInfo, Notification notification)
		{
			Console.WriteLine("Device Subscription Expired: " + platform.ToString() + " -> " + deviceInfo);
		}

		static void Events_OnChannelDestroyed(PlatformType platformType, int newChannelCount)
		{
			Console.WriteLine("Channel Destroyed for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}

		static void Events_OnChannelCreated(PlatformType platformType, int newChannelCount)
		{
			Console.WriteLine("Channel Created for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}
    }
}
