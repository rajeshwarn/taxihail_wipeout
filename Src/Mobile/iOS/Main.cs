using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Navigation;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.Data;
using Xamarin.Contacts;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using MonoTouch.FacebookConnect;

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

    public partial class AppDelegate : MvxApplicationDelegate, IMvxServiceConsumer<IMvxStartNavigation>
    {
        private bool _callbackFromFB = false;
        private bool _isStarting = false;

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            _isStarting = true;
            ThreadHelper.ExecuteInThread (() => MonoTouch.ObjCRuntime.Runtime.StartWWAN (new Uri (new AppSettings ().ServiceUrl)));

            Background.Load (window, "Assets/background_full_nologo.png", false, 0, 0);          

            AppContext.Initialize (window);
            var @params = new Dictionary<string, string> ();
            if (options != null && options.ContainsKey (new NSString ("UIApplicationLaunchOptionsRemoteNotificationKey"))) {
                NSDictionary remoteNotificationParams = options.ObjectForKey(new NSString("UIApplicationLaunchOptionsRemoteNotificationKey")) as NSDictionary;
                if(remoteNotificationParams.ContainsKey(new NSString("orderId")))
                {
                    @params["orderId"] = remoteNotificationParams.ObjectForKey(new NSString ("orderId")).ToString();
                }
            }
			var setup = new Setup(this, new PhonePresenter( this, window ), @params );
            setup.Initialize();


            window.MakeKeyAndVisible();


			var start = this.GetService<IMvxStartNavigation>();
			start.Start();     

			window.RootViewController = AppContext.Current.Controller;
            return true;
        }

        private void SetUIDefaults()
        {
            var buttonAtt = new UITextAttributes{ TextColor = AppStyle.LightCorporateColor, TextShadowColor = UIColor.Clear };
            UIBarButtonItem.Appearance.SetTitleTextAttributes(buttonAtt, UIControlState.Normal);

        }
        // This method is required in iPhoneOS 3.0
        public override void OnActivated(UIApplication application)
        {
			FBAppCall.HandleDidBecomeActive();

            UIApplication.CheckForIllegalCrossThreadCalls=true;

            var locService = TinyIoCContainer.Current.Resolve<AbstractLocationService>();
            if ( locService != null )
            {
                locService.Start ();
            }

            ThreadHelper.ExecuteInThread ( () =>        MonoTouch.ObjCRuntime.Runtime.StartWWAN( new Uri ( new AppSettings().ServiceUrl ) ));

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
                JsConfig.RegisterTypeForAot< apcurium.MK.Booking.Google.Resources.ResultStatus>();
                JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.AddressComponentType>();
            } catch(NullReferenceException){
                // In the Simulator, a NullReferenceException is mysteriously thrown
            }
			JsConfig.RegisterTypeForAot<Coordinate>();
            JsConfig.RegisterTypeForAot<Contact>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.AddressComponent>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.Bounds>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.DirectionResult>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.Distance>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.Duration>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.Event>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.Geometry>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.GeoObj>();
            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.GeoResult>();



            if (!_callbackFromFB)
            {    

				if( !_isStarting &&  AppContext.Current.Controller != null && AppContext.Current.Controller.TopViewController is BookView )
				{
					var model = ((BookView)AppContext.Current.Controller.TopViewController).ViewModel;
                    model.Reset ();
                    if ( model.AddressSelectionMode != Data.AddressSelectionMode.PickupSelection )
                    {
                        model.ActivatePickup.Execute ();
                    }
                    model.Pickup.RequestCurrentLocationCommand.Execute ();
				}
            }
            else
            {
                _callbackFromFB = false;
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
			if (url.AbsoluteString.StartsWith("fb" + TinyIoCContainer.Current.Resolve<IAppSettings>().FacebookAppId + TinyIoCContainer.Current.Resolve<IAppSettings>().ApplicationName.ToLower().Replace( " ", string.Empty ) ))
			{
				_callbackFromFB = true;
				var handleOpenUrl = FBAppCall.HandleOpenURL(url, sourceApplication);
				return handleOpenUrl;
			}

			return false;
        }

        public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
        {
            var strFormat = new NSString("%@");
            var dt = new NSString(MonoTouch.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr(new MonoTouch.ObjCRuntime.Class("NSString").Handle, new MonoTouch.ObjCRuntime.Selector("stringWithFormat:").Handle, strFormat.Handle, deviceToken.Handle));
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