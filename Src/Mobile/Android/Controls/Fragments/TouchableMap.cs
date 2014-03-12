using System;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchableMap : SupportMapFragment
    {
        public View mOriginalContentView;

        public TouchableWrapper Surface;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup parent, Bundle savedInstanceState)
        {
            mOriginalContentView = base.OnCreateView(inflater, parent, savedInstanceState);

            var bestPosition = TinyIoCContainer.Current.Resolve<ILocationService>().BestPosition;
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>().Data;

            var latitude = bestPosition != null ? bestPosition.Latitude : settings.DefaultLatitude;
            var longitude = bestPosition != null ? bestPosition.Longitude : settings.DefaultLongitude;

            
            Map.MapType = GoogleMap.MapTypeNormal;
            Map.UiSettings.CompassEnabled = false;
            Map.UiSettings.ZoomControlsEnabled = false;
            Map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(latitude, longitude) , 12f));

            Surface = new TouchableWrapper(Activity);
            Surface.AddView(mOriginalContentView);
            return Surface;                          
        }

        public override View View
        {
            get
            {
                return mOriginalContentView;
            }
        }
    }

    public class TouchableWrapper: FrameLayout {

        public event EventHandler<MotionEvent> Touched;

        public TouchableWrapper(Context context) :
        base(context)
        {
        }

        public TouchableWrapper(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
        }

        public TouchableWrapper(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
        {
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (this.Touched != null)
            {
                this.Touched(this, e);
            }

            return base.DispatchTouchEvent(e);
        }
    }
}