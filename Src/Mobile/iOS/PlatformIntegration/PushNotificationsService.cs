using System;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PushNotificationsService: BaseService
    {
        public PushNotificationsService ()
        {
        }

        public void SaveDeviceToken(NSData deviceToken)
        {
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
            
            //There's probably a better way to do this
            var strFormat = new NSString("%@");
            var dt = new NSString(MonoTouch.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr(new MonoTouch.ObjCRuntime.Class("NSString").Handle, new MonoTouch.ObjCRuntime.Selector("stringWithFormat:").Handle, strFormat.Handle, deviceToken.Handle));
            var newDeviceToken = dt.ToString().Replace("<", "").Replace(">", "").Replace(" ", "");
            
            if (string.IsNullOrEmpty(oldDeviceToken))
            {
                base.UseServiceClient<PushNotificationsServiceClient>(service => {
                    //service.Register(newDeviceToken);
                });
            }
            else if(!deviceToken.Equals(newDeviceToken))
            {
                base.UseServiceClient<PushNotificationsServiceClient>(service => {
                    //service.Unregister(oldDeviceToken);
                    //service.Register(newDeviceToken);
                });
            }


            
            //Save device token now
            NSUserDefaults.StandardUserDefaults.SetString(newDeviceToken, "PushDeviceToken");
            
            Console.WriteLine("Device Token: " + newDeviceToken);
        }
    }
}

