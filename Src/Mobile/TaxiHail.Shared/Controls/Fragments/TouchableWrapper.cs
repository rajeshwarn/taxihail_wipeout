using System;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchableWrapper : FrameLayout 
    {
        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleDetector;

        private static DateTime _blockScrollUntilDate = DateTime.MinValue;

        public event EventHandler<MotionEvent> Touched;
        public Action<bool, float> ZoomBy;
        public Action<float, float> MoveBy;
        private bool _isGestuesEnabled;

        public bool IsGestuesEnabled
        {
            get { return _isGestuesEnabled; }
            set
            {
                _isGestuesEnabled = value;

                Alpha = value ? 1f : .3f;
            }
        }

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

            IsGestuesEnabled = true;
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (!IsGestuesEnabled)
            {
                // Map control disabled.
                return true;
            }

            if (Touched != null)
            {
                Touched(this, e);
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

                if (_blockScrollUntilDate >= DateTime.Now)
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
                _blockScrollUntilDate = DateTime.Now.AddMilliseconds (500);
                return true;
            }
        }
    }
}

