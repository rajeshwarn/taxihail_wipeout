using System;
using Android.Widget;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Views.Animations;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class SlidingPanel : LinearLayout, View.IOnTouchListener
    {
        private View _slideoutView;
        private SlideDownAnimation _animation;
        private float _startY;
        private float _lastY;
        private bool _goingDown;
        private float _initialHeight;
        private int SlideOutHeight
        {
            get { return MeasureSlideOutHeight(); }
        }

        int MeasureSlideOutHeight()
        {
            _slideoutView.Measure(MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            return _slideoutView.MeasuredHeight;
        }

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

        private bool _isInitialized = false;

        private void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                if (ChildCount != 2)
                {
                    throw new InvalidOperationException("The sliding panel must have exactly 2 child control");
                }

                _slideoutView = this.GetChildAt(0);

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

        public bool OnTouch(View v, MotionEvent e)
        {
            if (v != this || !this.IsEnabled)
            {
                return true;
            }

            if (e.Action == MotionEventActions.Down)
            {
                var p = (RelativeLayout.LayoutParams)LayoutParameters;
                _initialHeight = p.TopMargin;
                _startY = e.RawY;
                _lastY = _startY;
            }
            else if (e.Action == MotionEventActions.Move)
            {
                var y = e.RawY;
                var newHeight = _initialHeight + y - _startY;
                var openMargin = -1 * SlideOutHeight;
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
                var openMargin = -1 * SlideOutHeight;
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

        void OpenClose(bool open, bool animate, int? startMargin = null)
        {
            if (animate)
            {
                this.ClearAnimation();
                this.DrawingCacheEnabled = true;
                if (!startMargin.HasValue)
                {
                    startMargin = open ? -1 * SlideOutHeight : 0;
                }
                _animation = new SlideDownAnimation(this, startMargin.Value, open ? 0 : -1 * SlideOutHeight, new DecelerateInterpolator());
                _animation.Duration = 600;
                _animation.AnimationEnd += delegate
                {
                    OpenClose(open, false);
                };
                this.StartAnimation(_animation);
            }
            else
            {
                SetTopMargin(open ? 0 : -1 * SlideOutHeight);
            }
        }

        private void SetTopMargin(int m)
        {

            var p = (RelativeLayout.LayoutParams)this.LayoutParameters;
            p.TopMargin = m;
            this.LayoutParameters = p;

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

        private bool _isEnabled { get; set; }
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
    }
}

