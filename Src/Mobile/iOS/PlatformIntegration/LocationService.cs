using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.CoreLocation;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Diagnostics;
using MonoTouch.Foundation;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationService : AbstractLocationService
    {
        private CLLocationManager _locationManager;
        private LocationManagerDelegate _locationDelegate;
        private static ILogger LoggerService {get{ return TinyIoCContainer.Current.Resolve<ILogger>();}}
                
        public override bool IsLocationServicesEnabled
        {
            get { return CLLocationManager.Status == CLAuthorizationStatus.Authorized && CLLocationManager.LocationServicesEnabled ;}
        }

        public LocationService()
        {
            if (_locationManager != null)
            {
                _locationManager.StartUpdatingLocation();
                return;
            }
            
            _locationManager = new CLLocationManager();
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            _locationManager.DistanceFilter = -1;
            _locationDelegate = new LocationManagerDelegate();
            _locationManager.Delegate = _locationDelegate;
            Positions = _locationDelegate;
        }
                
        public override void Start()
        {   
            _locationManager.StartUpdatingLocation();
        }
        
        public override void Stop ()
        {
            _locationManager.StopUpdatingLocation ();
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
