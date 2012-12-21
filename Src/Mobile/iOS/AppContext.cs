using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using MonoTouch.EventKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppContext // : IAppContext
    {
        private static AppContext _current;

        public static void Initialize(UIWindow window)
        {
            _current = new AppContext(window);
        }

        public static AppContext Current
        {
            get { return _current; }
        }
        
        private UINavigationController _controller;


        private AppContext(UIWindow window)
        {
            Window = window;
        }

        public UIWindow Window  { get; private set; }
            

//        public string LoggedInEmail
//        {
//            get
//            {
//                Console.WriteLine("getting : LoggedInEmail");
//                return NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.LoggedInEmail");
//            }
//            set
//            { 
//
//                if (value != LoggedInEmail)
//                {
//                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(value, "MK.Booking.Cache.LoggedInEmail");
//                }
//            }
//        }
//
//        public string LoggedInPassword
//        {
//            get { return NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.LoggedInPassword"); }
//            set
//            {
//                if (value != LoggedInPassword)
//                {
//                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(value, "MK.Booking.Cache.LoggedInPassword");
//                }
//            }
//        }
//                       
//        public bool ReceiveMemoryWarning
//        {
//            get;
//            set;
//        }
        
		public UINavigationController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }


//		public string ServerName {
//			get {
//				return NSUserDefaults.StandardUserDefaults.StringForKey("TaxiMobile.ServerName");
//			}
//			set {
//				NSUserDefaults.StandardUserDefaults.SetStringOrClear( value, "TaxiMobile.ServerName" );
//			}
//		}
//
//		public string ServerVersion {
//			get {
//				//return TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetServerVersion();
//				return "1.0" ;
//			}
//		}        
        
    }
	
    
}


