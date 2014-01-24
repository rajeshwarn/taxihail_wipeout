using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PushNotificationService: BaseService, IPushNotificationService
    {

        public void RegisterDeviceForPushNotifications (bool force = false)
        {
            if (force) {
                NSUserDefaults.StandardUserDefaults.SetString (string.Empty, "PushDeviceToken");
            }

            UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Alert
                                                                               | UIRemoteNotificationType.Badge
                                                                               | UIRemoteNotificationType.Sound);
        }

        public void SaveDeviceToken(string deviceToken)
        {
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
            
            //There's probably a better way to do this
            var newDeviceToken = deviceToken.Replace("<", "").Replace(">", "").Replace(" ", "");
            
            if (string.IsNullOrEmpty(oldDeviceToken))
            {
				UseServiceClientTask<PushNotificationRegistrationServiceClient>(service => service.Register(newDeviceToken, PushNotificationServicePlatform.Apple));
            }
            else if(!oldDeviceToken.Equals(newDeviceToken))
            {
				UseServiceClientTask<PushNotificationRegistrationServiceClient>(service => service.Replace(oldDeviceToken, newDeviceToken, PushNotificationServicePlatform.Apple));
            }
                        
            //Save device token now
            NSUserDefaults.StandardUserDefaults.SetString(newDeviceToken, "PushDeviceToken");
            
            Console.WriteLine("Device Token: " + newDeviceToken);
        }
    }
}

