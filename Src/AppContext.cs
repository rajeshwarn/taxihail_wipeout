using System;
using System.IO;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using MobileTaxiApp.Infrastructure;

using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public class AppContext : IAppContext
	{
		private static AppContext _current;

		public static void Initialize ( UIWindow window)
		{
			_current = new AppContext (window);
		}


		public static AppContext Current {
			get { return _current; }
		}
		
	
#if DEBUG
#else
		private CLLocationManager _locationManager;
#endif
		
		private RootTabController _controller;

		private CLLocation _currrentLocation;

		private AccountData _loggedUser;

		private AppContext (UIWindow window)
		{
			Window = window;
			
			#if DEBUG
			//_currrentLocation = new CLLocation (45.522702, -73.624036);
			_currrentLocation = new CLLocation (45.5323870130381, -73.6015319824219);
			
			#else
			_currrentLocation = new CLLocation (0, 0);
			_locationManager = new CLLocationManager ();
			_locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
			_locationManager.DistanceFilter = -1;
			_locationManager.Delegate = new AppLocationManagerDelegate (this);
			_locationManager.StartUpdatingLocation ();
			#endif
			
			ServiceLocator.Current.RegisterSingleInstance2<IAppContext> (this);
			ServiceLocator.Current.RegisterSingleInstance2<IAppSettings> (new AppSettings ());
			ServiceLocator.Current.RegisterSingleInstance2<ILogger> (new LoggerWrapper ());
		}


		public UIWindow Window  {get;private set;}
			
		public void ResetPosition ()
		{
			
			
			
			#if DEBUG
			//_currrentLocation = new CLLocation (45.522702, -73.624036);
			_currrentLocation = new CLLocation (45.5323870130381, -73.6015319824219);
			#else
			_locationManager.StopUpdatingLocation ();
			_currrentLocation = new CLLocation (0, 0);
			_locationManager.StartUpdatingLocation ();
			#endif
			
			
		}

		public void UpdateLoggedInUser (AccountData data, bool syncWithServer)
		{
			Logger.LogMessage( "UpdateLoggedInUser" );
			if (data != null)
			{
				Logger.LogMessage( "UpdateLoggedInUser != null" );
				_loggedUser = data;
								
				NSUserDefaults.StandardUserDefaults.SetSerializedObject<AccountData> (data, "TaxiMobile.AccountData.CurrentUser" );
								
				if (syncWithServer)
				{
					ServiceLocator.Current.GetInstance<IAccountService> ().UpdateUser (data);
				}
			}
			else{
				Logger.LogMessage( "UpdateLoggedInUser == null" );
			}
		}
		
		public void SignOutUser()
		{			
			Logger.LogMessage( "SignOutUser" );		
			_loggedUser = null;												
			NSUserDefaults.StandardUserDefaults.SetStringOrClear (null, "TaxiMobile.AccountData.CurrentUser");				
		}
		public AccountData LoggedUser {
			get {
								
				if (_loggedUser == null) 
				{
					_loggedUser = NSUserDefaults.StandardUserDefaults.GetSerializedObject<AccountData> ("TaxiMobile.AccountData.CurrentUser" );
				}
				return _loggedUser;
			}
		}

		public string LastEmailUsed {
			get { return NSUserDefaults.StandardUserDefaults.StringForKey ("TaxiMobile.LastEmailUsed"); }
			set { NSUserDefaults.StandardUserDefaults.SetStringOrClear (value, "TaxiMobile.LastEmailUsed"); }
		}
		
		public bool WarnEstimate {
			get {
				string  val = NSUserDefaults.StandardUserDefaults.StringForKey ("TaxiMobile.WarnEstimate");
				bool r = true;
				if (  ( val == null ) || ( !bool.TryParse( val , out r )  ) )
				{
					return true;
				}
				else
				{
					return r;
				}
			}
			set { NSUserDefaults.StandardUserDefaults.SetStringOrClear (value.ToString(), "TaxiMobile.WarnEstimate"); }
		}
		
		public bool ReceiveMemoryWarning
		{
			get;set;
		}
		
		public int? LastOrder {
			get {
				var lOrder = NSUserDefaults.StandardUserDefaults.StringForKey ("TaxiMobile.LastOrder");
				if (lOrder.HasValue ())
				{
					int r;
					if (int.TryParse (lOrder, out r))
					{
						return r;
					}
				}
				return null;
			}
			set {
				if (value.HasValue)
				{
					NSUserDefaults.StandardUserDefaults.SetStringOrClear (value.ToString (), "TaxiMobile.LastOrder");
				}

				else
				{
					NSUserDefaults.StandardUserDefaults.SetStringOrClear (null, "TaxiMobile.LastOrder");
				}
			}
		}
		public RootTabController Controller {
			get { return _controller; }
			set { _controller = value; }
		}

		public void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
		{
			CLLocation currentLocation;
			
			#if DEBUG
			currentLocation = new CLLocation (45.522702, -73.624036);
			#else
			currentLocation = newLocation;
			#endif
			_currrentLocation = currentLocation;
			
			
		}

		public CLLocation CurrrentLocation {
			get { return _currrentLocation; }
		}
		
		
	}


	public class AppLocationManagerDelegate : CLLocationManagerDelegate
	{
		private AppContext _context;

		public AppLocationManagerDelegate (AppContext context)
		{
			_context = context;
		}

		public static DateTime NSDateToDateTime (MonoTouch.Foundation.NSDate date)
		{
			return (new DateTime (2001, 1, 1, 0, 0, 0)).AddSeconds (date.SecondsSinceReferenceDate);
		}


		public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
		{
			
			
			//var newLocationEventDate = newLocation.Timestamp;
			
			double secondHowRecent = newLocation.Timestamp.SecondsSinceReferenceDate - NSDate.Now.SecondsSinceReferenceDate;
			
			if (((_context.CurrrentLocation.Coordinate.Latitude == 0) || (_context.CurrrentLocation.Coordinate.Longitude == 0) || (_context.CurrrentLocation.HorizontalAccuracy > newLocation.HorizontalAccuracy)) && (secondHowRecent < -0.0 && secondHowRecent > -10.0))
			{
				_context.UpdatedLocation (manager, newLocation, oldLocation);
				Console.WriteLine ("********************UPDATING LOCATION**************************");
				Console.WriteLine ("Lat : " + newLocation.Coordinate.Latitude.ToString ());
				Console.WriteLine ("Long : " + newLocation.Coordinate.Longitude.ToString ());
				Console.WriteLine ("HAcc : " + newLocation.HorizontalAccuracy.ToString ());
				Console.WriteLine ("VAcc : " + newLocation.VerticalAccuracy.ToString ());
				
				
				
				
			}
			
		}
		
		
		
	}
	
}


