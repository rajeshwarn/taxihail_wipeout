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
    public class SlidingPanel : LinearLayout, Android.Views.View.IOnTouchListener
    {

        private View _visibleView;
        private View _slideoutView;
        private SlideDownAnimation _animation;
        private float _startY;
        private float _lastY;
        private bool _goingDown;
        private float _initialHeight;
        private int _slideOutHeight
        {
            get { return this.LayoutParameters.Height - _visibleView.LayoutParameters.Height; }
        }

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public SlidingPanel(Context context)
            : base(context)
        {
            SetOnTouchListener(this);
        }
        
        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public SlidingPanel(Context context, IAttributeSet attrs)
            : base(context, attrs)
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
                _visibleView = this.GetChildAt(1);                

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

            var p = (RelativeLayout.LayoutParams)this.LayoutParameters;
            if (e.Action == MotionEventActions.Down)
            {
                _initialHeight = p.TopMargin;
                _startY = e.RawY;
                _lastY = _startY;            
            }
            else if (e.Action == MotionEventActions.Move)
            {
                var y = e.RawY;
                var newHeight = _initialHeight + y - _startY; 
             //   var openMargin = -1 * _slideoutView.LayoutParameters.Height;    
                var openMargin = -1 * _slideOutHeight;              
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
              //  var openMargin = -1 * _slideoutView.LayoutParameters.Height;       
                var openMargin = -1 * _slideOutHeight;                
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
                  //  startMargin = open ? -1 * _slideoutView.LayoutParameters.Height : 0;
                    startMargin = open ? -1 * _slideOutHeight : 0;
                }
                //_animation = new SlideDownAnimation(this, startMargin.Value, open ? 0 : -1 * _slideoutView.LayoutParameters.Height, new DecelerateInterpolator());
                _animation = new SlideDownAnimation(this, startMargin.Value, open ? 0 : -1 * _slideOutHeight, new DecelerateInterpolator());
                _animation.Duration = 600;    
                _animation.AnimationEnd += delegate
                {
                    OpenClose(open, false );
                };
                this.StartAnimation(_animation);
            }
            else
            {
               // SetTopMargin(open ? 0 : -1 * _slideoutView.LayoutParameters.Height);
                SetTopMargin(open ? 0 : -1 * _slideOutHeight);
            }



        }

        private void SetTopMargin(int m)
        {

            var p = (RelativeLayout.LayoutParams)this.LayoutParameters;
            p.TopMargin = m;
            this.LayoutParameters = p;

            PostInvalidate();
           // RefreshAllParents(this);


        }

        private void RefreshAllParents(View v)
        {
            v.PostInvalidate();
//            v.RequestLayout();
//            if (v is ViewGroup)
//            {
//                ((ViewGroup)v).ForceLayout();
//            }
//
//            if (v.Parent is View)
//            {
//                RefreshAllParents((View)v.Parent);
//            }

        }

        public void Close()
        {
            OpenClose(false, true);
        }


        public void Open()
        {
            OpenClose(true, true);
        }

        public void Toggle()
        {
            bool isClosed = (( RelativeLayout.LayoutParams ) this.LayoutParameters).TopMargin < -1;

            if ( isClosed )
            {
                Open();
            }
            else{
                Close();
            }

        }

        private bool _isEnabled { get; set; }
        public bool IsEnabled
        {
            get {return _isEnabled;}
            set 
            { 
                if ( _isEnabled != value )
                {
                    _isEnabled = value;
                    if ( IsEnabled )
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

