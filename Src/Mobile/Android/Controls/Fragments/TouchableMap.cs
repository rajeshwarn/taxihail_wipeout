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
using Android.App;
using Android.Support.V4.View;

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
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;

			var latitude = bestPosition != null ? bestPosition.Latitude : settings.GeoLoc.DefaultLatitude;
			var longitude = bestPosition != null ? bestPosition.Longitude : settings.GeoLoc.DefaultLongitude;

            Map.MapType = GoogleMap.MapTypeNormal;
            Map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(latitude, longitude) , 12f));

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

        public override View View
        {
            get
            {
                return mOriginalContentView;
            }
        }
    }

    public class TouchableWrapper : FrameLayout 
    {
        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleDetector;

        private static DateTime BlockScrollUntilDate = DateTime.MinValue;

        public event EventHandler<MotionEvent> Touched;
        public Action<bool, float> ZoomBy;
        public Action<float, float> MoveBy;

        public TouchableWrapper(Context context) :
        base(context)
        {
            Initialize ();
        }

        public TouchableWrapper(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
            Initialize ();
        }

        public TouchableWrapper(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
        {
            Initialize ();
        }

        private void Initialize()
        {
            _gestureDetector = new GestureDetector (Context, new GestureListener (this));
            _scaleDetector = new ScaleGestureDetector (Context, new ScaleListener (this));
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (this.Touched != null)
            {
                this.Touched(this, e);
            }

            _gestureDetector.OnTouchEvent (e);
            _scaleDetector.OnTouchEvent (e);

            return true;
        }

        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            private readonly TouchableWrapper _view;
            public GestureListener (TouchableWrapper view)
            {   
                _view = view;
            }

            public override bool OnScroll (MotionEvent firstDownMotionEvent, MotionEvent moveMotionEvent, float distanceX, float distanceY)
            {
                if (firstDownMotionEvent.PointerCount > 1 || moveMotionEvent.PointerCount > 1)
                {
                    // don't scroll if we have more than one finger
                    return false;
                }

                if (BlockScrollUntilDate >= DateTime.Now)
                {
                    return false;
                }

                _view.MoveBy.Invoke (distanceX, distanceY);
                return true;
            }

            public override bool OnDoubleTap (MotionEvent e)
            {
                if (e.PointerCount > 1)
                {
                    // Zooming out on double tap with multitouch
                    _view.ZoomBy.Invoke (true, -1f);
                }
                else
                {
                    // Zooming in on double tap with one finger
                    _view.ZoomBy.Invoke (true, 1f);
                }

                return true;
            }
        }

        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener 
        {
            private readonly TouchableWrapper _view;
            public ScaleListener (TouchableWrapper view)
            {   
                _view = view;
            }

            public override bool OnScale (ScaleGestureDetector detector)
            {
                _view.ZoomBy.Invoke (false, (detector.ScaleFactor - 1f) * 4);
                BlockScrollUntilDate = DateTime.Now.AddMilliseconds (500);
                return true;
            }
        }
    }
}