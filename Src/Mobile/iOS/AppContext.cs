using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppContext : IAppContext
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
            

        public string LastEmail
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("TaxiMobile.LastEmailUsed"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetStringOrClear(
                    value,
                    "TaxiMobile.LastEmailUsed"
                );
            }
        }

        public string LoggedInEmail
        {
            get
            {
                Console.WriteLine("getting : LoggedInEmail");
                return NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.LoggedInEmail");
            }
            set
            { 

                if (value != LoggedInEmail)
                {
                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(value, "MK.Booking.Cache.LoggedInEmail");
                }
            }
        }

        public string LoggedInPassword
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.LoggedInPassword"); }
            set
            {
                if (value != LoggedInPassword)
                {
                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(value, "MK.Booking.Cache.LoggedInPassword");
                }
            }
        }
        
        public bool WarnEstimate
        {
            get
            {
                string val = NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.WarnEstimate");
                bool r = true;
                if ((val == null) || (!bool.TryParse(val, out r)))
                {
                    return true;
                }
                else
                {
                    return r;
                }
            }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetStringOrClear(
                    value.ToString(),
                    "MK.Booking.Cache.WarnEstimate"
                );
            }
        }
        
        public bool ReceiveMemoryWarning
        {
            get;
            set;
        }
        
        public Guid? LastOrder
        {
            get
            {
                var lOrder = NSUserDefaults.StandardUserDefaults.StringForKey("MK.Booking.Cache.LastOrder");
                if (lOrder.HasValue())
                {
                    Guid r;
                    if (Guid.TryParse(lOrder, out r))
                    {
                        return r;
                    }
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(
                        value.ToString(),
                        "MK.Booking.Cache.LastOrder"
                    );
                }
                else
                {
                    NSUserDefaults.StandardUserDefaults.SetStringOrClear(
                        null,
                        "MK.Booking.Cache.LastOrder"
                    );
                }
            }
        }

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


