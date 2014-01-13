using System;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public class LocationService : AbstractLocationService
    {
        private readonly LocationListener _locationListener;
        private readonly LocationManager _locationManager;
        private bool _isStarted;

        public LocationService()
        {
            _locationManager = (LocationManager) Application.Context.GetSystemService(Context.LocationService);
            _locationListener = new LocationListener();

            Positions = _locationListener;
        }

        public bool IsNetworkProviderEnabled
        {
            get { return _locationManager.IsProviderEnabled(LocationManager.NetworkProvider); }
        }

        public bool IsGpsProviderEnabled
        {
            get { return _locationManager.IsProviderEnabled(LocationManager.GpsProvider); }
        }

        public override bool IsLocationServicesEnabled
        {
            get { return IsNetworkProviderEnabled && IsGpsProviderEnabled; }
        }

        public override Position BestPosition
        {
            get { return _locationListener.BestPosition; }
        }

        public override Position LastKnownPosition
        {
            get { return _locationListener.LastKnownPosition; }
        }

        public bool IsLocationServiceEnabled
        {
            get
            {
                return _locationManager.IsProviderEnabled(LocationManager.NetworkProvider)
                       || _locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            }
        }

        public override bool IsStarted
        {
            get { return _isStarted; }
        }

        public override void Stop()
        {
            if (IsStarted)
            {
                _isStarted = false;
                _locationManager.RemoveUpdates(_locationListener);
            }
        }

        public override void Start()
        {
            if (IsStarted)
            {
                return;
            }

            if (!IsLocationServiceEnabled)
            {
                throw new Exception("Please enable location services!!");
            }

            if (IsNetworkProviderEnabled)
            {
                _locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, _locationListener,
                    Looper.MainLooper);
            }

            if (IsGpsProviderEnabled)
            {
                _locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, _locationListener,
                    Looper.MainLooper);
            }
            _isStarted = true;
        }
    }
}