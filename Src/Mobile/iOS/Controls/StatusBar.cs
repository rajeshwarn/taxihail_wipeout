using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public sealed class ShadowView : UIView
    {
        public ShadowView ()
        {
            Layer.MasksToBounds = false;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOpacity = 0.5f;
            Layer.ShadowRadius = 1f;
            Layer.ShadowOffset = new CGSize (0f, -1f);
        }
        
        public void Resize ()
        {
            var size = new CGSize (UIScreen.MainScreen.Bounds.Width, 0);
            var curlFactor = 2.0f;
            var shadowDepth = 5.0f;
            var path = new UIBezierPath ();
            path.MoveTo (new CGPoint (-2f, size.Height - 1f));
            path.AddLineTo (new CGPoint (size.Width + 2f, size.Height - 1f));
            path.AddLineTo (new CGPoint (size.Width + 2f, size.Height - 1f + shadowDepth));
            path.AddCurveToPoint (new CGPoint (-2f, size.Height - 1f + shadowDepth),
                                 new CGPoint (size.Width + 2f - curlFactor, size.Height - 1f + shadowDepth - curlFactor),
                                 new CGPoint (curlFactor, size.Height - 1f + shadowDepth - curlFactor));
            
            Layer.ShadowPath = path.CGPath;
        }
    }
    
    [Register ("StatusBar")]
    public class StatusBar : UIView
    {
        private ShadowView _shadowView;
        private UIView _visibleView;
        private UIView _slideoutView;

        private UIView _assignedVisibleView;
        private UIView _assignedSlideoutView;

        private bool _wasTouched;

        private nfloat _minHeight;
        private nfloat _maxHeight;

        public StatusBar(IntPtr handle) : base(handle)
        {
        }
        
		public void SetShadow ()
        {
            if (_shadowView == null) 
            {
                _shadowView = new ShadowView (); 
                _shadowView.Frame = new CGRect (0, Bounds.Height, UIScreen.MainScreen.Bounds.Width, 200);
                _shadowView.BackgroundColor = UIColor.Clear;
                AddSubview (_shadowView);
            }
            _shadowView.Frame = new CGRect (0, Bounds.Height, UIScreen.MainScreen.Bounds.Width, 10);
            _shadowView.Resize ();
        }
        
        void SetVisibleView ()
        {
            if (_visibleView == null) 
            {
                _visibleView = new UIView ();
                _visibleView.BackgroundColor = UIColor.Clear;
                AddSubview (_visibleView);

                _assignedVisibleView.RemoveFromSuperview ();
                _visibleView.AddSubview ( _assignedVisibleView );
                _assignedVisibleView.Frame = new CGRect(0,0, UIScreen.MainScreen.Bounds.Width , _visibleView.Bounds.Height ); 
            }
           
            _visibleView.Frame = new CGRect (0, Bounds.Height - _minHeight, UIScreen.MainScreen.Bounds.Width, _minHeight);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set 
            { 
                if ( _isEnabled != value )
                {
                    _isEnabled = value;
                    if (IsEnabled && !_wasTouched)
                    {
                        SlideOut();
                    }
                    else if (!IsEnabled)
                    {
                        SlideIn();
                    }
                }
            }
        }

        private void SetSlideoutView ()
        {
            if (_slideoutView == null) 
            {
                _slideoutView = new UIView ();
                _slideoutView.BackgroundColor = UIColor.Clear;
                AddSubview (_slideoutView);

                _assignedSlideoutView.RemoveFromSuperview ();
                _slideoutView.AddSubview ( _assignedSlideoutView );
                _assignedSlideoutView.Frame = new CGRect(0,0, UIScreen.MainScreen.Bounds.Width , _slideoutView.Bounds.Height ); 
            }

            var topVisibleView = Bounds.Height - _minHeight;
            var heightSlideOut = _maxHeight - _minHeight;
            
            _slideoutView.Frame = new CGRect (0, topVisibleView - heightSlideOut, UIScreen.MainScreen.Bounds.Width, heightSlideOut);
        }

		public void Initialize (UIView visibleView, UIView slidingView)
        {
            _assignedVisibleView = visibleView;
            _assignedSlideoutView = slidingView;

            UserInteractionEnabled = true;
            BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("rideStatusBarBackground.png"));
            
            ClipsToBounds = false;
            _minHeight = visibleView.Bounds.Height;
            _maxHeight = visibleView.Bounds.Height + slidingView.Bounds.Height;
            
            SetShadow ();           
            SetVisibleView ();
            SetSlideoutView ();

            SetHeight (_minHeight, false);
        }
        
        public void SlideOut ()
        {
            SetHeight (_maxHeight, true);
        }

        public void SlideIn ()
        {
            SetHeight (_minHeight, true);
        }

        CGPoint _startPt;
        private nfloat _initialHeight;
        
        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {
            var t = touches.AnyObject as UITouch;
            if (t != null && IsEnabled) 
            {
                _initialHeight = Bounds.Height;
                _startPt = t.LocationInView (this);
                _lastPoint = _startPt;
            }
            
            base.TouchesBegan (touches, evt);
        }
        
        private bool _goingDown;
        private CGPoint _lastPoint;
        
        public override void TouchesMoved (NSSet touches, UIEvent evt)
        {
            var t = touches.AnyObject as UITouch;
            if (t != null && IsEnabled) 
            {
                var p = t.LocationInView (this);
                var newHeight = _initialHeight + p.Y - _startPt.Y;
                
                if ((newHeight >= _minHeight) && (newHeight <= _maxHeight)) 
                {
                    _goingDown = p.Y > _lastPoint.Y;
                    _lastPoint = p;
                    SetHeight (newHeight, false);
                }
            }
            base.TouchesMoved (touches, evt);
        }
        
		public void SetMaxHeight(nfloat height)
		{
			_maxHeight = height;

            if (!_wasTouched)
            {
			    SetHeight (_maxHeight, false);
            }
        }

        public void SetMinHeight(nfloat height)
        {
            _minHeight = height;

            if (!_wasTouched)
            {
                SetHeight (_maxHeight, false);
            }
        }

        public void SetHeight (nfloat height, bool animate)
        {
            Action changeSize = () => 
            {
                Frame = new CGRect (Frame.X, Frame.Y, UIScreen.MainScreen.Bounds.Width, height);
                SetShadow ();
                SetVisibleView ();
                SetNeedsDisplay ();
                SetSlideoutView ();
            };

            if (animate) 
            {
                BeginAnimations ("setheight");
                SetAnimationDuration (0.5);
                changeSize ();
                CommitAnimations ();
            } 
            else 
            {
                changeSize ();
            }
        }
        
        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {
            base.TouchesEnded (touches, evt);
            _wasTouched = true;
            
            if (_goingDown) 
            {    
                SetHeight (_maxHeight, true);
            }
            else 
            {
                SetHeight (_minHeight, true);               
            }
        }
    }
}

