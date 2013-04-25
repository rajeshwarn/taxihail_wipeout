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

		public void Stop()
		{            
			_locationManager.RemoveUpdates(LocationListener);
		}
		
		public void Start()
		{    		
			if(!IsLocationServiceEnabled)
			{
				throw new Exception("Please enable location services!!");
			}
			if(_locationManager.IsProviderEnabled( LocationManager.NetworkProvider ) )
			{
				_locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, LocationListener);
			}
			else if(_locationManager.IsProviderEnabled( LocationManager.GpsProvider ))
			{				
				_locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, LocationListener);
			}

			Positions = LocationListener;
		}

		public IObservable<Position> GetNextBest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public IObservable<Position> GetNextPosition (TimeSpan timeout, float maxAccuracy)
		{
			throw new NotImplementedException ();
		}

		public bool IsLocationServicesEnabled {
			get {
				throw new NotImplementedException ();
			}
		}

		public Position BestPosition {
			get {
				throw new NotImplementedException ();
			}
		}

		private LocationManager _locationManager;
		private LocationListener LocationListener;   
		
		public IObservable<Position> Positions { get; private set; }
		
		public Position LastKnownPosition
		{
			get { return LocationListener.LastKnownPosition;	}  
		}
		
		public bool IsLocationServiceEnabled
		{
			get{
				return _locationManager.IsProviderEnabled( LocationManager.NetworkProvider ) 
					||  _locationManager.IsProviderEnabled( LocationManager.GpsProvider );
			}
		}


		
	}
}
