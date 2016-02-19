using System;
using Android.Content;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("apcurium.mk.booking.mobile.client.controls.TouchableMap")]
    public class TouchableMap : MapFragment
    {
        public View mOriginalContentView;

        public TouchableWrapper Surface;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup parent, Bundle savedInstanceState)
        {
            mOriginalContentView = base.OnCreateView(inflater, parent, savedInstanceState);

            var locationService = TinyIoCContainer.Current.Resolve<ILocationService>();
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;

            var initialPosition = locationService.GetInitialPosition();

            Map.MapType = GoogleMap.MapTypeNormal;
            Map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(initialPosition.Latitude, initialPosition.Longitude) , 12f));

            // disable gestures on the map since we're handling them ourselves
            Map.UiSettings.CompassEnabled = false;
            Map.UiSettings.ZoomControlsEnabled = false;
            Map.UiSettings.ZoomGesturesEnabled = false;
            Map.UiSettings.RotateGesturesEnabled = false;
            Map.UiSettings.TiltGesturesEnabled = false;
            Map.UiSettings.ScrollGesturesEnabled = false;

            Surface = new TouchableWrapper(Activity);
            Surface.AddView(mOriginalContentView);
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
                return mOriginalContentView;
            }
        }
    }
}