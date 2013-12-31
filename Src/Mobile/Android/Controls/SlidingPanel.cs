using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public sealed class SlidingPanel : LinearLayout, View.IOnTouchListener
    {
        private SlideDownAnimation _animation;
        private bool _goingDown;
        private float _initialHeight;
        private bool _isInitialized;
        private float _lastY;
        private View _slideoutView;
        private float _startY;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public SlidingPanel(Context context) : base(context)
        {
            SetOnTouchListener(this);
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public SlidingPanel(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetOnTouchListener(this);
        }

        private int SlideOutHeight
        {
            get { return MeasureSlideOutHeight(); }
        }

        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (IsEnabled)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (v != this || !IsEnabled)
            {
                return true;
            }

            if (e.Action == MotionEventActions.Down)
            {
                var p = (RelativeLayout.LayoutParams) LayoutParameters;
                _initialHeight = p.TopMargin;
                _startY = e.RawY;
                _lastY = _startY;
            }
            else if (e.Action == MotionEventActions.Move)
            {
                var y = e.RawY;
                var newHeight = _initialHeight + y - _startY;
                var openMargin = -1*SlideOutHeight;
                if ((newHeight >= openMargin) && (newHeight <= 0))
                {
                    _goingDown = y > _lastY;
                    _lastY = y;
                    SetTopMargin(Convert.ToInt32(newHeight));
                }
            }
            else if (e.Action == MotionEventActions.Up)
            {
                var y = e.RawY;
                var newHeight = _initialHeight + y - _startY;
                var openMargin = -1*SlideOutHeight;
                if ((newHeight >= openMargin) && (newHeight <= 0))
                {
                    OpenClose(_goingDown, true, Convert.ToInt32(newHeight));
                }
                else
                {
                    OpenClose(_goingDown, false);
                }
            }
            return true;
        }

        private int MeasureSlideOutHeight()
        {
            _slideoutView.Measure(MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
                MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            return _slideoutView.MeasuredHeight;
        }

        private void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                if (ChildCount != 2)
                {
                    throw new InvalidOperationException("The sliding panel must have exactly 2 child control");
                }

                _slideoutView = GetChildAt(0);

                OpenClose(false, false);
            }
        }

        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);
            if (visibility == ViewStates.Visible)
            {
                Initialize();
            }
        }

        private void OpenClose(bool open, bool animate, int? startMargin = null)
        {
            if (animate)
            {
                ClearAnimation();
                DrawingCacheEnabled = true;
                if (!startMargin.HasValue)
                {
                    startMargin = open ? -1*SlideOutHeight : 0;
                }
                _animation = new SlideDownAnimation(this, startMargin.Value, open ? 0 : -1*SlideOutHeight,
                    new DecelerateInterpolator());
                _animation.Duration = 600;
                _animation.AnimationEnd += delegate { OpenClose(open, false); };
                StartAnimation(_animation);
            }
            else
            {
                SetTopMargin(open ? 0 : -1*SlideOutHeight);
            }
        }

        private void SetTopMargin(int m)
        {
            var p = (RelativeLayout.LayoutParams) LayoutParameters;
            p.TopMargin = m;
            LayoutParameters = p;

            PostInvalidate();
        }

        public void Close()
        {
            OpenClose(false, true);
        }

        public void Open()
        {
            OpenClose(true, true);
        }
    }
}