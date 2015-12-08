using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using CoreAnimation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("OverlayButton")]
    public class OverlayButton : CommandButton
    {
        private const float RadiusCorner = 2f;
        private UIView _shadowView;
		private UIColor fillColorNormal = UIColor.White.ColorWithAlpha(0.8f);
		private UIColor fillColorPressed = UIColor.Gray.ColorWithAlpha(0.8f);

        public OverlayButton(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = fillColorNormal;

            this.TouchDown += (sender, e) => 
			{
                BackgroundColor = fillColorPressed;
				this.SetNeedsDisplay();
			};				
        }

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
            BackgroundColor = fillColorNormal;
			this.SetNeedsDisplay();
		}

		// do not remove this commented code since design is there
		// see ticket MKTAXI-3611 why commented
        /*public override void Draw (CGRect rect)
        {  
			if (!Hidden)
			{
				DrawStroke();
			}

			SetNeedsDisplay();
        }*/

        private CAShapeLayer GetMaskForRoundedCorners()
        {
            var roundedRectanglePath = UIBezierPath.FromRoundedRect (Bounds, RadiusCorner);
            var biggerRect = Bounds.Copy().Grow(5);

            var maskPath = new UIBezierPath();
            maskPath.MoveTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AppendPath(roundedRectanglePath);

            var maskForRoundedCorners = new CAShapeLayer();
            var newPath = new CGPath();
            newPath.AddRect(biggerRect);
            newPath.AddPath(maskPath.CGPath);
            maskForRoundedCorners.Path = newPath;
            maskForRoundedCorners.FillRule = CAShapeLayer.FillRuleEvenOdd;

            newPath.Dispose();
            maskPath.Dispose();
            roundedRectanglePath.Dispose();

            return maskForRoundedCorners;
        }

        private CGPath GetShadowPath(CGRect biggerRect)
        {
            var shadowPath = new UIBezierPath();
            shadowPath.MoveTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));;
            shadowPath.AppendPath(UIBezierPath.FromRoundedRect (Bounds, RadiusCorner));
            shadowPath.UsesEvenOddFillRule = true;

            return shadowPath.CGPath;
        }

        protected virtual void DrawStroke()
        {
            this.Layer.Mask = GetMaskForRoundedCorners();

            DrawShadow();
        }

        private void DrawShadow()
        {
            ClearShadowIfNecessary();

            var biggerRect = Bounds.Copy().Grow(2);

            _shadowView = new UIView(Frame);
            _shadowView.Layer.MasksToBounds = false;
            _shadowView.Layer.ShadowColor = UIColor.Black.CGColor;
            _shadowView.Layer.ShadowOpacity = 0.3f;
            _shadowView.Layer.ShadowOffset = new CGSize(0f, 0f);
            _shadowView.Layer.ShadowPath = GetShadowPath(biggerRect);
            _shadowView.Layer.ShouldRasterize = true;   
            _shadowView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
            this.Superview.InsertSubviewBelow(_shadowView, this);   
        }

        private void ClearShadowIfNecessary()
        {
            if (_shadowView != null)
            {
                _shadowView.RemoveFromSuperview();
                _shadowView = null;
            }
        }

        public override bool Hidden
        {
            get { return base.Hidden; }
            set
            {
                base.Hidden = value;

                if (Hidden)
                {
                    // Remove dropshadow
                    if (_shadowView != null)
                    {
                        _shadowView.RemoveFromSuperview();
                        _shadowView = null;
                    }
                }
            }
        }
    }
}

