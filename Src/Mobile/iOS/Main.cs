using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Views;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using Cirrious.MvvmCross.Touch.Platform;
using apcurium.MK.Booking.Mobile.Data;
using Xamarin.Contacts;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Configuration.Impl;
using MonoTouch.FacebookConnect;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

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
        private bool _callbackFromFb;
        private bool _isStarting;

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            _isStarting = true;

            if (!UIHelper.IsOS7orHigher)
            {
                UIApplication.SharedApplication.StatusBarHidden = false;
            }

            Background.Load (window, "Assets/background_full_nologo.png", false);          

            var @params = new Dictionary<string, string> ();
            if (options != null && options.ContainsKey (new NSString ("UIApplicationLaunchOptionsRemoteNotificationKey"))) {
                var remoteNotificationParams = options.ObjectForKey(new NSString("UIApplicationLaunchOptionsRemoteNotificationKey")) as NSDictionary;
                if(remoteNotificationParams != null && remoteNotificationParams.ContainsKey(new NSString("orderId")))
                {
                    @params["orderId"] = remoteNotificationParams.ObjectForKey(new NSString ("orderId")).ToString();
                }
            }

			var setup = new Setup(this, window);
            setup.Initialize();

			var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start(@params);

            window.MakeKeyAndVisible();

            return true;
        }

        // This method is required in iPhoneOS 3.0
        public override void OnActivated(UIApplication application)
        {
			UIApplication.CheckForIllegalCrossThreadCalls=true;

			//Facebook init
			FBAppCall.HandleDidBecomeActive();

            var locService = TinyIoCContainer.Current.Resolve<AbstractLocationService>();
            if ( locService != null )
            {
                locService.Start ();
            }

            ThreadHelper.ExecuteInThread (() => Runtime.StartWWAN( new Uri ( Mvx.Resolve<AppSettings>().Data.ServiceUrl )));

            Logger.LogMessage("OnActivated");

            try {
                //Register Enums
                JsConfig.RegisterTypeForAot<OrderStatus>();
                JsConfig.RegisterTypeForAot<CoordinatePrecision>();
                JsConfig.RegisterTypeForAot<CoordinateRefreshTime>();
                JsConfig.RegisterTypeForAot<AddressType>();
                JsConfig.RegisterTypeForAot<OrganizationType>();
                JsConfig.RegisterTypeForAot<RelationshipType>();
                JsConfig.RegisterTypeForAot<EmailType>();
                JsConfig.RegisterTypeForAot<PhoneType>();
                JsConfig.RegisterTypeForAot<PushNotificationServicePlatform>();
                JsConfig.RegisterTypeForAot<PaymentMethod> ();
                JsConfig.RegisterTypeForAot<InstantMessagingService>();
                JsConfig.RegisterTypeForAot< ResultStatus>();
                JsConfig.RegisterTypeForAot<AddressComponentType>();
            } catch(NullReferenceException){
                // In the Simulator, a NullReferenceException is mysteriously thrown
            }
			JsConfig.RegisterTypeForAot<Coordinate>();
            JsConfig.RegisterTypeForAot<Contact>();
            JsConfig.RegisterTypeForAot<AddressComponent>();
            JsConfig.RegisterTypeForAot<Bounds>();
            JsConfig.RegisterTypeForAot<DirectionResult>();
            JsConfig.RegisterTypeForAot<Distance>();
            JsConfig.RegisterTypeForAot<Duration>();
            JsConfig.RegisterTypeForAot<Event>();
            JsConfig.RegisterTypeForAot<Geometry>();
            JsConfig.RegisterTypeForAot<GeoObj>();
            JsConfig.RegisterTypeForAot<GeoResult>();

            if (!_callbackFromFb)
            {    
                var navController = Mvx.Resolve<UINavigationController>();
                if( !_isStarting &&  navController != null && navController.TopViewController is BookView )
				{
                    var model = ((BookView)navController.TopViewController).ViewModel;
                    model.Reset ();
                    if ( model.AddressSelectionMode != AddressSelectionMode.PickupSelection )
                    {
                        model.ActivatePickup.Execute ();
                    }
                    model.Pickup.RequestCurrentLocationCommand.Execute ();
				}
            }
            else
            {
                _callbackFromFb = false;
            }           

            _isStarting = false;
        }

        public override void DidEnterBackground (UIApplication application)
        {
            var locService = TinyIoCContainer.Current.Resolve<AbstractLocationService>() as LocationService ;
            if ( locService != null )
            {
                locService.Stop();
            }

            base.DidEnterBackground (application);
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            Logger.LogMessage("ReceiveMemoryWarning");
        }
        
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
			Console.WriteLine(url.ToString());
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            if (url.AbsoluteString.StartsWith("fb" + settings.Data.FacebookAppId + settings.Data.ApplicationName.ToLower().Replace( " ", string.Empty ) ))
			{
                _callbackFromFb = true;
				return FBAppCall.HandleOpenURL(url, sourceApplication);
			}

			return false;
        }

        public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
        {
            var strFormat = new NSString("%@");
            var dt = new NSString(Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr(new Class("NSString").Handle, new Selector("stringWithFormat:").Handle, strFormat.Handle, deviceToken.Handle));
            new PushNotificationService().SaveDeviceToken(dt);
        }
        
        public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
        {
            Console.WriteLine("Failed to register for notifications");
        }
        
        public override void ReceivedRemoteNotification (UIApplication application, NSDictionary userInfo)
        {
            Console.WriteLine("Received Remote Notification!");
        }
    }
}