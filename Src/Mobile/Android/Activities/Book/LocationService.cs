using System;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore.Droid;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Cirrious.CrossCore.Droid.Platform;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public class LocationService : BaseLocationService
    {
        private readonly LocationListenerManager _locationListeners;
        private readonly LocationManager _locationManager;
        private readonly IMessageService _messageService;
        private bool _isStarted;

        public LocationService()
        {
            _locationManager = (LocationManager) Application.Context.GetSystemService(Context.LocationService);
            _locationListeners = new LocationListenerManager();
            _messageService = new MessageService(Application.Context);
            Positions = _locationListeners;
        }

        public bool IsNetworkProviderEnabled
        {
            get { return _locationManager.IsProviderEnabled(LocationManager.NetworkProvider); }
        }

        public bool IsGpsProviderEnabled
        {
            get { 
                //_locationManager.IsProviderEnabled not working for gps
                //http://stackoverflow.com/questions/10117587/gps-is-not-enabled-but-isproviderenabled-is-returning-true
                var context = TinyIoCContainer.Current.Resolve<IMvxAndroidGlobals> ().ApplicationContext;
                var gps = Android.Provider.Settings.Secure.GetString (context.ContentResolver,Android.Provider.Settings.Secure.LocationProvidersAllowed).Contains( "gps" ); 
                return gps;
            }
        }

        public override bool IsLocationServicesEnabled
        {
            get { return IsNetworkProviderEnabled && IsGpsProviderEnabled; }
        }

        public override Position BestPosition
        {
            get { return _locationListeners.BestPosition; }
        }

        public override Position LastKnownPosition
        {
            get { return _locationListeners.LastKnownPosition; }
        }

        public bool IsLocationServiceEnabled
        {
            get
            {            
                return IsNetworkProviderEnabled || IsGpsProviderEnabled;
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
                _locationManager.RemoveUpdates(_locationListeners.GpsListener);
                _locationManager.RemoveUpdates(_locationListeners.NetworkListener);
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
                var localize = TinyIoCContainer.Current.Resolve<ILocalization>();

				var dontShowLocationWarning = (string)TinyIoCContainer.Current.Resolve<ICacheService>("UserAppCache").Get<string>("WarningLocationServiceDontShow");

                if (dontShowLocationWarning != "yes")
                {
                    _messageService.ShowMessage(localize["WarningLocationServiceTitle"], localize["WarningLocationService"],
                        "Settings", delegate
                    { 
                        var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>(); 
                        topActivity.Activity.StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                    },
						localize["WarningLocationServiceDontShow"], () => TinyIoCContainer.Current.Resolve<ICacheService>("UserAppCache").Set("WarningLocationServiceDontShow", "yes"),
                        localize["WarningLocationServiceCancel"], delegate
                    {
                    } 
                    );
                }
            }

            if (IsNetworkProviderEnabled)
            {
                _locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, _locationListeners.NetworkListener,
                    Looper.MainLooper);

                _locationListeners.OnLocationChanged(_locationManager.GetLastKnownLocation(LocationManager.NetworkProvider));
            }

            if (IsGpsProviderEnabled)
            {
                _locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, _locationListeners.GpsListener,
                    Looper.MainLooper);
                _locationListeners.OnLocationChanged(_locationManager.GetLastKnownLocation(LocationManager.GpsProvider));
            }
            _isStarted = true;
        }

        private Position ToPosition( Location location )
        {
            if (location != null)
            {
                return new Position()
                {
                    Time = location.Time.ToDateTime(),
                    Error = location.Accuracy,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };
            }
            return null;
        }
    }
}