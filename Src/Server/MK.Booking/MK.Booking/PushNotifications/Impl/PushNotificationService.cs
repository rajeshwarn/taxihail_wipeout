using System;
using System.Collections.Generic;
using System.IO;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Newtonsoft.Json;
using PushSharp;
using PushSharp.Google;
using PushSharp.Apple;
using PushSharp.Blackberry;
using PushSharp.Core;
using ILogger = apcurium.MK.Common.Diagnostic.ILogger;
using Newtonsoft.Json.Linq;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class PushNotificationService : IPushNotificationService
    {
        private const string iOSApp = "com.mobile-knowledge.TaxiHail.ios";
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        Dictionary<string, AppPushBrokers> _apps = new Dictionary<string, AppPushBrokers>();
        private bool _androidStarted;
        private bool _blackberryStarted;
        private bool _appleStarted;

        public PushNotificationService(IServerSettings serverSettings, ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public void Send(string alert, IDictionary<string, object> data, string deviceToken, PushNotificationServicePlatform platform)
        {

            switch (platform)
            {
                case PushNotificationServicePlatform.Apple:
                    EnsureApnsStarted();
                    SendAppleNotification(alert, data, deviceToken);
                    break;
                case PushNotificationServicePlatform.Android:
                    EnsureGcmStarted();
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


        private void EnsureApnsStarted()
        {
            if (_appleStarted)
            {
                return;
            }

            _appleStarted = true;
            ApnsConfiguration configuration;

#if DEBUG
            const ApnsConfiguration.ApnsServerEnvironment environment = ApnsConfiguration.ApnsServerEnvironment.Sandbox;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                _serverSettings.ServerData.APNS.DevelopmentCertificatePath);

#else
            const ApnsConfiguration.ApnsServerEnvironment environment = ApnsConfiguration.ApnsServerEnvironment.Production;
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                _serverSettings.ServerData.APNS.ProductionCertificatePath);
#endif

            // Apple settings placed next for development purpose. (Crashing the method when certificate is missing.)
            var appleCert = File.ReadAllBytes(certificatePath);

            configuration = new ApnsConfiguration(environment, appleCert, _serverSettings.ServerData.APNS.CertificatePassword);
            _apps.Add(iOSApp, new AppPushBrokers { Apns = new ApnsServiceBroker(configuration) });

            _apps[iOSApp].Apns.OnNotificationSucceeded += OnApnsNotificationSucceeded;
            _apps[iOSApp].Apns.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var x = ex as ApnsNotificationException;

                        // Deal with the failed notification
                        ApnsNotification n = x.Notification;
                        string description = "Message: " + x.Message + " Data:" + x.Data.ToString();

                        _logger.LogMessage($"Notification Failed: ID={n.Identifier}, Desc={description}");
                    }
                    else if (ex is ApnsConnectionException)
                    {
                        var x = ex as ApnsConnectionException;
                        string description = "Message: " + x.Message + " Data:" + x.Data.ToString();

                        _logger.LogMessage($"Notification Failed: Connection exception, Desc={description}");

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        LogDeviceSubscriptionExpiredException((DeviceSubscriptionExpiredException)ex);
                    }
                    else if (ex is RetryAfterException)
                    {
                        LogRetryAfterException((RetryAfterException)ex);
                    }
                    else
                    {
                        _logger.LogMessage("Notification Failed for some (Unknown Reason)");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            _apps[iOSApp].Apns.Start();

        }

        private void EnsureGcmStarted()
        {
            if (_androidStarted)
            {
                return;
            }

            _androidStarted = true;

            string appId = _serverSettings.ServerData.GCM.PackageName;

            GcmConfiguration configuration = new GcmConfiguration(
                _serverSettings.ServerData.GCM.SenderId,
                _serverSettings.ServerData.GCM.APIKey,
                appId);

            _apps.Add(appId, new AppPushBrokers { Gcm = new GcmServiceBroker(configuration) });

            _apps[appId].Gcm.OnNotificationSucceeded += OnGcmNotificationSucceeded;

            _apps[appId].Gcm.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var x = ex as GcmNotificationException;

                        // Deal with the failed notification
                        GcmNotification n = x.Notification;
                        string description = x.Description;

                        _logger.LogMessage($"Notification Failed: ID={n.MessageId}, Desc={description}");
                    }
                    else if (ex is GcmMulticastResultException)
                    {

                        var x = ex as GcmMulticastResultException;

                        foreach (var succeededNotification in x.Succeeded)
                        {
                            _logger.LogMessage($"Notification Failed: ID={succeededNotification.MessageId}");
                        }

                        foreach (var failedKvp in x.Failed)
                        {
                            GcmNotification n = failedKvp.Key;
                            var e = failedKvp.Value as GcmNotificationException;

                            _logger.LogMessage($"Notification Failed: ID={n.MessageId}, Desc={e.Description}");
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        LogDeviceSubscriptionExpiredException((DeviceSubscriptionExpiredException)ex);
                    }
                    else if (ex is RetryAfterException)
                    {
                        LogRetryAfterException((RetryAfterException)ex);
                    }
                    else
                    {
                        _logger.LogMessage("Notification Failed for some (Unknown Reason)");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            _apps[appId].Gcm.Start();
        }

        private void EnsureBlackberryStarted()
        {
            if (_blackberryStarted)
            {
                return;
            }

            _blackberryStarted = true;

            string appId = _serverSettings.ServerData.BBNotificationSettings.AppId;

            BlackberryConfiguration configuration = new BlackberryConfiguration(
                appId, 
                _serverSettings.ServerData.BBNotificationSettings.Password);

            configuration.OverrideSendUrl(_serverSettings.ServerData.BBNotificationSettings.Url);

            _apps.Add(appId, new AppPushBrokers { Bb = new BlackberryServiceBroker(configuration) });

            _apps[appId].Bb.OnNotificationSucceeded += OnBlackBeryNotificationSucceeded;
            _apps[appId].Bb.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is BlackberryNotificationException)
                    {
                        var x = ex as BlackberryNotificationException;

                        // Deal with the failed notification
                        BlackberryNotification n = x.Notification;
                        string description = "Message: " + x.Message + " Data:" + x.Data.ToString();
                        _logger.LogMessage($"Notification Failed: ID={n.PushId}, Desc={description}");
                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        LogDeviceSubscriptionExpiredException((DeviceSubscriptionExpiredException)ex);
                    }
                    else if (ex is RetryAfterException)
                    {
                        LogRetryAfterException((RetryAfterException)ex);
                    }
                    else
                    {
                        _logger.LogMessage("Notification Failed for some (Unknown Reason)");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            _apps[appId].Bb.Start();
        }

        #region event handlers
        private void OnBlackBeryNotificationSucceeded(BlackberryNotification notification)
        {
            _logger.LogMessage("Sent: " + notification.Content.ToString());
        }

        private void OnGcmNotificationSucceeded(GcmNotification notification)
        {
            _logger.LogMessage("Sent: " + notification.Notification.ToString());
        }

        private void OnApnsNotificationSucceeded(ApnsNotification notification)
        {
            _logger.LogMessage("Sent: " + notification.Payload.ToString());
        }

        #endregion

        private void SendAndroidNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            GcmNotification notification = new GcmNotification() { DelayWhileIdle = false };
            notification.RegistrationIds.Add(registrationId);
            notification.CollapseKey = Guid.NewGuid().ToString();
            notification.DelayWhileIdle = false;
            notification.Data = JObject.Parse(JsonConvert.SerializeObject(payload));

            _apps[_serverSettings.ServerData.GCM.PackageName].Gcm.QueueNotification(notification);
        }

        private void SendAppleNotification(string alert, IDictionary<string, object> data, string deviceToken)
        {

            var notification = new ApnsNotification( deviceToken, JObject.Parse(JsonConvert.SerializeObject(data)));
            // what about with alert
            // what about with sount
            _apps[iOSApp].Apns.QueueNotification(notification);            

        }

        private void SendBBNotification(string alert, IDictionary<string, object> data, string registrationId)
        {
            var payload = new Dictionary<string, object>(data);
            payload["alert"] = alert;

            var notif = new BlackberryNotification();
            notif.Recipients.Add(new BlackberryRecipient(registrationId));
            notif.Content = new BlackberryMessageContent(JsonConvert.SerializeObject(payload));
            notif.SourceReference = _serverSettings.ServerData.BBNotificationSettings.AppId;

            _apps[_serverSettings.ServerData.BBNotificationSettings.AppId].Bb.QueueNotification(notif);
        }

        private void LogRetryAfterException(RetryAfterException ex)
        {
            // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
            _logger.LogMessage($"Rate Limited, don't send more until after {ex.RetryAfterUtc}");
        }
        private void LogDeviceSubscriptionExpiredException(DeviceSubscriptionExpiredException ex)
        {
            string oldId = ex.OldSubscriptionId;
            string newId = ex.NewSubscriptionId;

            _logger.LogMessage($"Device RegistrationId Expired: {oldId}");

            if (!string.IsNullOrEmpty(newId))
            {
                // If this value isn't null, our subscription changed and we should update our database
                _logger.LogMessage($"Device RegistrationId Changed To: {newId}");
            }

        }

    }
}