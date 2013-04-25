using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Android.Locations;
using Android.OS;
using MK.Common.Android;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	public class LocationService : AbstractLocationService
	{		
		public LocationService()
		{			
			_locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);     
			LocationListener = new LocationListener();
		}

		bool _isStarted;

		public override void Stop()
		{            
			if(_isStarted)
			{
				_locationManager.RemoveUpdates(LocationListener);
			}
		}
		
		public override void Start()
		{   
			if(_isStarted)
			{
				return;
			}

			if(!IsLocationServiceEnabled)
			{
				throw new Exception("Please enable location services!!");
			}
			if(IsNetworkProviderEnabled)
			{
				_locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, LocationListener, Looper.MainLooper);
			}
			if(IsGpsProviderEnabled)
			{				
				_locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, LocationListener, Looper.MainLooper);
			}
			_isStarted = true;
			Positions = LocationListener;
		}

		public bool IsNetworkProviderEnabled {
			get {
				return _locationManager.IsProviderEnabled( LocationManager.NetworkProvider );
			}
		}
		public bool IsGpsProviderEnabled {
			get {
				return _locationManager.IsProviderEnabled( LocationManager.GpsProvider );
			}
		}

		public override bool IsLocationServicesEnabled {
			get {
				return IsNetworkProviderEnabled && IsGpsProviderEnabled;
			}
		}

		public override Position BestPosition {
			get {
				return LocationListener.BestPosition;
			}
		}
		
		public override  Position LastKnownPosition
		{
			get { return LocationListener.LastKnownPosition;	}  
		}

		private LocationManager _locationManager;
		private LocationListener LocationListener;   


		
		public bool IsLocationServiceEnabled
		{
			get{
				return _locationManager.IsProviderEnabled( LocationManager.NetworkProvider ) 
					||  _locationManager.IsProviderEnabled( LocationManager.GpsProvider );
			}
		}


		
	}
}
