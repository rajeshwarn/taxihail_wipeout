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
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;
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

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            ThreadHelper.ExecuteInThread ( () =>        MonoTouch.ObjCRuntime.Runtime.StartWWAN( new Uri ( new AppSettings().ServiceUrl ) ));

            ///UIApplication.CheckForIllegalCrossThreadCalls = false;

            Background.Load(window, "Assets/background_full_nologo.png", false, 0, 0);          

			AppContext.Initialize(window);
            
			var setup = new Setup(this, new PhonePresenter( this, window ) );
            setup.Initialize();


            window.MakeKeyAndVisible();

			var start = this.GetService<IMvxStartNavigation>();
			start.Start();     
            
            ThreadHelper.ExecuteInThread(() => TinyIoCContainer.Current.Resolve<IAccountService>().EnsureListLoaded());

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

            ThreadHelper.ExecuteInThread ( () =>        MonoTouch.ObjCRuntime.Runtime.StartWWAN( new Uri ( new AppSettings().ServiceUrl ) ));

            Logger.LogMessage("OnActivated");

            JsConfig.RegisterTypeForAot<OrderStatus>();
            JsConfig.RegisterTypeForAot<OrderStatusDetail>();
			JsConfig.RegisterTypeForAot<Coordinate>();
			JsConfig.RegisterTypeForAot<CoordinatePrecision>();
			JsConfig.RegisterTypeForAot<CoordinateRefreshTime>();
			JsConfig.RegisterTypeForAot<AddressType>();
			JsConfig.RegisterTypeForAot<OrganizationType>();
			JsConfig.RegisterTypeForAot<RelationshipType>();
			JsConfig.RegisterTypeForAot<InstantMessagingService>();
			JsConfig.RegisterTypeForAot<EmailType>();
			JsConfig.RegisterTypeForAot<PhoneType>();
			JsConfig.RegisterTypeForAot<Contact>();



            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Api.Contract.Resources.AppSetting>();


            JsConfig.RegisterTypeForAot< apcurium.MK.Booking.Google.Resources.ResultStatus>();


            JsConfig.RegisterTypeForAot<apcurium.MK.Booking.Google.Resources.AddressComponentType>();
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
				if( AppContext.Current.Controller != null && AppContext.Current.Controller.TopViewController is BookView )
				{
					var model = ((BookView)AppContext.Current.Controller.TopViewController).ViewModel;
					model.Maybe( () => model.Initialize() );
				}
            }
            else
            {
                _callbackFromFB = false;
            }           
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            AppContext.Current.ReceiveMemoryWarning = true;
            Logger.LogMessage("ReceiveMemoryWarning");
        }
        
        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            Console.WriteLine(url.ToString());
            if (url.AbsoluteString.StartsWith("fb" + TinyIoCContainer.Current.Resolve<IAppSettings>().FacebookAppId ))
            {
                _callbackFromFB = true;
                return (TinyIoCContainer.Current.Resolve<IFacebookService>() as FacebookServiceMT).HandleOpenURL(application, url);
            }
            return false;               
        }
        
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return HandleOpenURL(application, url);
        }       


    }
    
}