using System;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PushNotificationService: BaseService, IPushNotificationService
    {
        public PushNotificationService ()
        {
        }

        public void RegisterDeviceForPushNotifications ()
        {

            UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Alert
                                                                               | UIRemoteNotificationType.Badge
                                                                               | UIRemoteNotificationType.Sound);
        }

        public void SaveDeviceToken(string deviceToken)
        {
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
            
            //There's probably a better way to do this
            var strFormat = new NSString("%@");
            var newDeviceToken = deviceToken.ToString().Replace("<", "").Replace(">", "").Replace(" ", "");
            
            if (string.IsNullOrEmpty(oldDeviceToken))
            {
                base.UseServiceClient<PushNotificationRegistrationServiceClient>(service => {
                    service.Register(newDeviceToken, PushNotificationServicePlatform.Apple);
                });
            }
            else if(true /*!oldDeviceToken.Equals(newDeviceToken)*/)
            {
                base.UseServiceClient<PushNotificationRegistrationServiceClient>(service => {
                    service.Replace(oldDeviceToken, newDeviceToken, PushNotificationServicePlatform.Apple);
                });
            }
                        
            //Save device token now
            NSUserDefaults.StandardUserDefaults.SetString(newDeviceToken, "PushDeviceToken");
            
            Console.WriteLine("Device Token: " + newDeviceToken);
        }
    }
}

