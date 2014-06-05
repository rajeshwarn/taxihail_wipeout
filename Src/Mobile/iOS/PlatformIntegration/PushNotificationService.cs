using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Helper;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class PushNotificationService: BaseService, IPushNotificationService
    {
		ICacheService _cacheService;

		public PushNotificationService (ICacheService cacheService)
		{
			_cacheService = cacheService;
		}

		public void RegisterDeviceForPushNotifications (bool force = false)
        {
            if (force) {
                NSUserDefaults.StandardUserDefaults.SetString (string.Empty, "PushDeviceToken");
            }

			CheckIfUserHasDiablePushNotification ();

            UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Alert
                                                                               | UIRemoteNotificationType.Badge
                                                                               | UIRemoteNotificationType.Sound);


        }

		void CheckIfUserHasDiablePushNotification ()
		{
			//check only after the first run (because on the first one we have not yet ask for push notification permission)
			const string pushFirstStart = "pushFirstStart";
			var firstStartFlag = _cacheService.Get<object>(pushFirstStart);

			if (firstStartFlag == null) 
			{
				_cacheService.Set (pushFirstStart, new object ());

			} else if(IsPushDisabled())
			{
				var localize = TinyIoCContainer.Current.Resolve<ILocalization>();

				var warningPushDontShow = "WarningPushDontShow";
				var dontShowPushWarning = (string)_cacheService.Get<string>(warningPushDontShow);

				if (dontShowPushWarning != "yes")
				{
					MessageHelper.Show(localize["WarningPushServiceTitle"],
						localize["WarningPushServiceMessage"],
						localize["WarningPushServiceAction"],
						() => _cacheService.Set (warningPushDontShow, "yes")
					);
				}
			}
		}

		bool IsPushDisabled ()
		{
			var enabledTypes = UIApplication.SharedApplication.EnabledRemoteNotificationTypes;

			if (((enabledTypes & UIRemoteNotificationType.Alert) != UIRemoteNotificationType.Alert)) {
				return true;
			}
			return false;
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

