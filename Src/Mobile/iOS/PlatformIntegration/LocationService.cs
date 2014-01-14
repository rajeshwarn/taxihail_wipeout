using Cirrious.CrossCore.Touch;
using MonoTouch.CoreLocation;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class LocationService : AbstractLocationService
    {
        private readonly CLLocationManager _locationManager;
        private readonly LocationManagerDelegate _locationDelegate;

        public override bool IsLocationServicesEnabled
        {
            get { return CLLocationManager.Status == CLAuthorizationStatus.Authorized && CLLocationManager.LocationServicesEnabled ;}
        }

        bool _isStarted;
        public override bool IsStarted {
            get {
                return _isStarted;
            }
        }


        public LocationService()
        {
            _locationManager = new CLLocationManager();
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            _locationManager.DistanceFilter = 10;//The minimum distance (measured in meters) a device must move horizontally before an update event is generated.
            _locationDelegate = new LocationManagerDelegate();
            _locationManager.Delegate = _locationDelegate;
            Positions = _locationDelegate;
        }
                
        public override void Start()
        {   
            if(_isStarted)
            {
                return;
            }

            if (_locationManager.Location != null) {
                
                _locationDelegate.BestPosition = new Position
                {
                    Error = (float)_locationManager.Location.HorizontalAccuracy,
                    Time = _locationManager.Location.Timestamp.ToDateTimeUtc(),
                    Latitude = _locationManager.Location.Coordinate.Latitude,
                    Longitude = _locationManager.Location.Coordinate.Longitude
                };
            }

            _locationManager.StartUpdatingLocation();
            _isStarted = true;

        }
        
        public override void Stop ()
        {   
            if(_isStarted)
            {
                _locationManager.StopUpdatingLocation ();
                _isStarted = false;
            }

        }
        
        public override Position LastKnownPosition
        {
            get { return _locationDelegate.LastKnownPosition; }
        }
                
        public override Position BestPosition
        {
            get { return _locationDelegate.BestPosition; }
        }

       

    }
}
