using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class ShadowView : UIView
    {
        
        public ShadowView ()
        {
            Layer.MasksToBounds = false;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOpacity = 0.5f;
            Layer.ShadowRadius = 1f;
            Layer.ShadowOffset = new SizeF (0f, -1f);
            
        }
        
        public void Resize ()
        {
            var size = new SizeF (Bounds.Size.Width, 0);
            var curlFactor = 2.0f;
            var shadowDepth = 5.0f;
            UIBezierPath path = new UIBezierPath ();
            path.MoveTo (new PointF (-2f, size.Height - 1f));
            path.AddLineTo (new PointF (size.Width + 2f, size.Height - 1f));
            path.AddLineTo (new PointF (size.Width + 2f, size.Height - 1f + shadowDepth));
            path.AddCurveToPoint (new PointF (-2f, size.Height - 1f + shadowDepth),
                                 new PointF (size.Width + 2f - curlFactor, size.Height - 1f + shadowDepth - curlFactor),
                                 new PointF (curlFactor, size.Height - 1f + shadowDepth - curlFactor));
            
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

        private float _minHeight;
        private float _maxHeight;
        
        public StatusBar ()
        {
            //Initialize();
        }
        
        public StatusBar (IntPtr handle) : base(  handle )
        {
            //Initialize();
        }
        
        void SetShadow ()
        {
            if (_shadowView == null) {
                _shadowView = new ShadowView (); 
                _shadowView.Frame = new RectangleF (0, this.Bounds.Height, this.Bounds.Width, 200);
                _shadowView.BackgroundColor = UIColor.Clear;
                AddSubview (_shadowView);
            }
            _shadowView.Frame = new RectangleF (0, this.Bounds.Height, this.Bounds.Width, 10);
            _shadowView.Resize ();
        }
        
        void SetVisibleView ()
        {
            if (_visibleView == null) {
                _visibleView = new UIView ();
                _visibleView.BackgroundColor = UIColor.DarkGray;


                AddSubview (_visibleView);

                _assignedVisibleView.RemoveFromSuperview ();
                _visibleView.AddSubview ( _assignedVisibleView );
                _assignedVisibleView.Frame = new RectangleF(0,0, _visibleView.Bounds.Width , _visibleView.Bounds.Height ); 

            }
           
            _visibleView.Frame = new RectangleF (0, this.Bounds.Height - _minHeight, this.Bounds.Width, _minHeight);
        }

        public bool isEnabled{get;set;}
        
        void SetSlideoutView ()
        {
            if (_slideoutView == null) {
                _slideoutView = new UIView ();
                _slideoutView.BackgroundColor = UIColor.LightGray;
                AddSubview (_slideoutView);

                _assignedSlideoutView.RemoveFromSuperview ();
                _slideoutView.AddSubview ( _assignedSlideoutView );
                _assignedSlideoutView.Frame = new RectangleF(0,0, _slideoutView.Bounds.Width , _slideoutView.Bounds.Height ); 

            }
            var topVisibleView = this.Bounds.Height - _minHeight;
            var heightSlideOut = _maxHeight - _minHeight;
            
            _slideoutView.Frame = new RectangleF (0, topVisibleView - heightSlideOut, this.Bounds.Width, heightSlideOut);
        }

        public void Initialize (UIView visibleView, UIView slidingView)
        {

            _assignedVisibleView = visibleView;
            _assignedSlideoutView = slidingView;

            UserInteractionEnabled = true;
            BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/rideStatusBarBackground.png"));
            
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

        


        PointF _startPt;
        private float _initialHeight;
        
        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {
            UITouch t = touches.AnyObject as UITouch;
            if (t != null && this.isEnabled) {
                _initialHeight = this.Bounds.Height;
                _startPt = t.LocationInView (this);
                _lastPoint = _startPt;
                Console.WriteLine (_startPt.Y);
            }
            
            base.TouchesBegan (touches, evt);
        }
        
        private bool _goingDown;
        private PointF _lastPoint;
        
        public override void TouchesMoved (NSSet touches, UIEvent evt)
        {
            UITouch t = touches.AnyObject as UITouch;
            if (t != null && this.isEnabled) {
                var p = t.LocationInView (this);
                Console.WriteLine (p.Y);
                var newHeight = _initialHeight + p.Y - _startPt.Y;
                
                if ((newHeight >= _minHeight) && (newHeight <= _maxHeight)) {
                    
                    _goingDown = p.Y > _lastPoint.Y;
                    _lastPoint = p;
                    SetHeight (newHeight, false);
                }
            }
            base.TouchesMoved (touches, evt);
        }
        
        void SetHeight (float height, bool animate)
        {
            Action changeSize = () => 
            {
                this.Frame = new RectangleF (this.Frame.X, this.Frame.Y, this.Bounds.Width, height);
                SetShadow ();
                SetVisibleView ();
                SetNeedsDisplay ();
                SetSlideoutView ();
            };
            if (animate) {
                UIView.BeginAnimations ("setheight");
                UIView.SetAnimationDuration (0.5);
                changeSize ();
                UIView.CommitAnimations ();
                
            } else {
                changeSize ();
            }
        }
        
        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {
            
            base.TouchesEnded (touches, evt);
            
            
            if (_goingDown) {
                
                SetHeight (_maxHeight, true);
                
            } else {
                SetHeight (_minHeight, true);               
            }
            
        }
        
        public override void Draw (System.Drawing.RectangleF rect)
        {
            
            base.Draw (rect);
            
            var rectpath = new UIBezierPath ();
            rectpath.MoveTo (new PointF (0, rect.Bottom));
            rectpath.AddLineTo (new PointF (rect.Right, rect.Bottom));
            
            UIColor.Black.SetStroke ();
            rectpath.LineWidth = 1f;
            rectpath.Stroke ();
            
        }
    }
}

