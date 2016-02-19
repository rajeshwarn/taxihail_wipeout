using System;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using Android.Runtime;
using Com.Mapbox.Mapboxsdk;
using Com.Mapbox.Mapboxsdk.Geometry;
using Com.Mapbox.Mapboxsdk.Views;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("apcurium.mk.booking.mobile.client.controls.TouchableMap")]
    public class TouchableMap : Fragment
    {
        public View mOriginalContentView;

        public TouchableWrapper Surface;

        public MapView Map;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup parent, Bundle savedInstanceState)
        {
            var locationService = TinyIoCContainer.Current.Resolve<ILocationService>();

            var initialPosition = locationService.GetInitialPosition();

            Map = new MapView(Activity.ApplicationContext,  settings.MapBoxKey);
            Map.OnCreate(savedInstanceState);

            Map.SetLogoVisibility((int)ViewStates.Gone);
            Map.SetAttributionVisibility((int)ViewStates.Gone);

            Map.StyleUrl = Com.Mapbox.Mapboxsdk.Constants.Style.MapboxStreets;
            Map.SetCenterCoordinate(new LatLngZoom(new LatLng(initialPosition.Latitude, initialPosition.Longitude), 12f), true);

            // disable gestures on the map since we're handling them ourselves
            Map.CompassEnabled = false;
            Map.ZoomEnabled = false;
            Map.RotateEnabled = false;
            Map.ScrollEnabled = false;

            Surface = new TouchableWrapper(Activity);
            Surface.AddView(Map);
            return Surface;                          
        }

	    public bool IsMapGestuesEnabled
	    {
		    get { return Surface.IsGestuesEnabled; } 
		    set { Surface.IsGestuesEnabled = value; }
	    }

	    public override View View
        {
            get
            {
                return Map;
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            Map.OnStop();
        }

        public override void OnStart()
        {
            base.OnStart();
            Map.OnStart();
        }

        public override void OnPause()
        {
            base.OnPause();
            Map.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();
            Map.OnResume();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Map.OnDestroy();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            Map.OnSaveInstanceState(outState);
        }
    }
}