using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using Com.Mapbox.Mapboxsdk.Maps;
using TinyIoC;

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
            Map = new MapView(Activity.ApplicationContext);
            Map.AccessToken = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.MapBoxKey;
			Map.StyleUrl = Com.Mapbox.Mapboxsdk.Constants.Style.MapboxStreets;

            Map.OnCreate(savedInstanceState);
            Surface = new TouchableWrapper(Activity);
            Surface.AddView(Map);
            return Surface;                          
        }

	    public bool IsMapGesturesEnabled
	    {
		    get { return Surface.IsGesturesEnabled; } 
		    set { Surface.IsGesturesEnabled = value; }
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