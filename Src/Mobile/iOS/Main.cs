using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.ViewModels;
using Foundation;
using ObjCRuntime;
using UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class Application
    {
        static void Main(string[] args)
        {
            try
			{
                UIApplication.Main(args);  
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        } 
    }

    public partial class AppDelegate : MvxApplicationDelegate
    {
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            UIApplication.SharedApplication.StatusBarHidden = false;                   

            var @params = new Dictionary<string, string> ();
            if (options != null && options.ContainsKey (new NSString ("UIApplicationLaunchOptionsRemoteNotificationKey"))) {
                var remoteNotificationParams = options.ObjectForKey(new NSString("UIApplicationLaunchOptionsRemoteNotificationKey")) as NSDictionary;
                if(remoteNotificationParams != null)
                {
                    if (remoteNotificationParams.ContainsKey(new NSString("orderId")))
                    {
                        @params["orderId"] = remoteNotificationParams.ObjectForKey(new NSString("orderId")).ToString();
                    }
                    if (remoteNotificationParams.ContainsKey(new NSString("isPairingNotification")))
                    {
                        @params["isPairingNotification"] = remoteNotificationParams.ObjectForKey(new NSString("isPairingNotification")).ToString();
                    }
                }
            }

            FacebookService.UIApplicationDelegateFinishedLaunching(app, options);

			var setup = new Setup(this, window);
            setup.Initialize();

            window.RootViewController = new SplashView();

			var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start(@params);

            window.MakeKeyAndVisible();

            return true;
        }

        // This method is required in iPhoneOS 3.0
        public override void OnActivated(UIApplication application)
        {		
			UIApplication.CheckForIllegalCrossThreadCalls=true;

            FacebookService.UIApplicationDelegateOnActivated();

            var locService = TinyIoCContainer.Current.Resolve<ILocationService>();
            if ( locService != null )
            {
                locService.Start ();
            }

            ThreadHelper.ExecuteInThread (() => Runtime.StartWWAN( new Uri ( Mvx.Resolve<IAppSettings>().Data.ServiceUrl )));

            Logger.LogMessage("OnActivated");
        }

        public override void DidEnterBackground (UIApplication application)
        {
            var locService = TinyIoCContainer.Current.Resolve<ILocationService>() as LocationService ;
            if ( locService != null )
            {
                locService.Stop();
            }

            base.DidEnterBackground (application);
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            #if DEBUG
            Logger.LogMessage("ReceiveMemoryWarning");
            #endif
        }
        
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            Logger.LogMessage(url.ToString());
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            if (url.AbsoluteString.StartsWith("fb" + FacebookService.FacebookApplicationID + settings.Data.TaxiHail.ApplicationName.ToLower().Replace(" ", string.Empty)))
			{
                return FacebookService.UIApplicationDelegateOpenURL(application, url, sourceApplication, annotation, settings.Data.TaxiHail.ApplicationName.ToLower().Replace(" ", string.Empty));
			}

			return false;
        }

        public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
        {
            var dt = deviceToken.ToString().Replace("<","").Replace(">","").Replace(" ","");
			TinyIoCContainer.Current.Resolve<IPushNotificationService>().SaveDeviceToken(dt);
        }
        
        public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
        {
            Logger.LogMessage("Error while registering push notification: "+ error.LocalizedDescription);
        }
        
        public override void ReceivedRemoteNotification (UIApplication application, NSDictionary userInfo)
        {
            Logger.LogMessage("Received Remote Notification!");
        }
    }
}