using Cirrious.CrossCore.Touch;
using MonoTouch.CoreLocation;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class LocationService : BaseLocationService
    {
        private readonly CLLocationManager _locationManager;
        private readonly LocationManagerDelegate _locationDelegate;

        bool _isStarted;
        public override bool IsStarted 
        { 
            get { return _isStarted; } 
        }

        public LocationService()
        {
            _locationManager = new CLLocationManager();
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            _locationManager.DistanceFilter = 10;//The minimum distance (measured in meters) a device must move horizontally before an update event is generated.
            if (UIHelper.IsOS8orHigher)
            {
                _locationManager.RequestWhenInUseAuthorization();
            }

            _locationDelegate = new LocationManagerDelegate();
            _locationManager.Delegate = _locationDelegate;
            Positions = _locationDelegate;
        }

        private bool LocationServiceIsEnabledAndAuthorized()
        {
            var enabled = CLLocationManager.LocationServicesEnabled;

            if (UIHelper.IsOS8orHigher)
            {
                return enabled
                    && (CLLocationManager.Status != CLAuthorizationStatus.AuthorizedWhenInUse
                        || CLLocationManager.Status != CLAuthorizationStatus.AuthorizedAlways);
            }
            else
            {
                return enabled
                    && CLLocationManager.Status != CLAuthorizationStatus.Authorized;
            }

        }

        public override void Start()
        {   
            if(_isStarted)
            {
                return;
            }

			var cacheService = TinyIoCContainer.Current.Resolve<ICacheService> ("UserAppCache");
			var firstStartLocationKey = "firstStartLocationKey" ;
			var firstStart = cacheService.Get<object> (firstStartLocationKey);

			if (firstStart == null
                || CLLocationManager.Status == CLAuthorizationStatus.NotDetermined) 
            {
				//don' t check the first time, the OS will ask permission after
				cacheService.Set (firstStartLocationKey, new object ());
			}
			else
            {
				//only warn if user has denied the app, if location are not enabled, th OS display a message
                if (LocationServiceIsEnabledAndAuthorized())
				{ 
					var localize = TinyIoCContainer.Current.Resolve<ILocalization>();

					var warningKey = "WarningLocationServiceDontShow";
					var dontShowLocationWarning = (string)cacheService.Get<string>(warningKey);

					if (dontShowLocationWarning != "yes")
					{
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(
							localize["WarningLocationServiceTitle"], 
							localize["WarningLocationService"],
							localize[warningKey], () => cacheService.Set (warningKey, "yes"),
							localize["WarningLocationServiceCancel"], () => {});
					}
				}
			}

			_locationManager.StartUpdatingLocation();

            if (_locationManager.Location != null) 
            {
                _locationDelegate.BestPosition = new Position
                {
                    Error = (float)_locationManager.Location.HorizontalAccuracy,
                    Time = _locationManager.Location.Timestamp.ToDateTimeUtc(),
                    Latitude = _locationManager.Location.Coordinate.Latitude,
                    Longitude = _locationManager.Location.Coordinate.Longitude
                };
            }

            _isStarted = true;
        }
        
        public override void Stop ()
        {   
            if(_isStarted)
            {			
                _locationManager.StopUpdatingLocation ();
                _isStarted = false;
				_locationDelegate.BestPosition = null;
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
