using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using CoreAnimation;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("OverlayView")]
    public class OverlayView : MvxView
    {
        private float _radiusCorner = 3f;
        private UIView _shadowView = null;

        public OverlayView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public OverlayView(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public OverlayView() : base()
        {
            Initialize();
        }

        private void Initialize()
        {

        }

        public override void Draw (CGRect rect)
        {          
            ClearShadowIfNecessary();

            if (!UIScreen.MainScreen.Bounds.Contains(Frame))
            {
                using (var context = UIGraphics.GetCurrentContext())
                {
                    context.ClearRect(rect);
                }

                return;
            }

            using (var context = UIGraphics.GetCurrentContext())
            {
                var fillColor = UIColor.White.ColorWithAlpha(0.9f);

                var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _radiusCorner);

                DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor);
                DrawStroke(fillColor.CGColor, rect);

                fillColor.Dispose();
                roundedRectanglePath.Dispose();
                context.Dispose();
            }
        }

        protected virtual void DrawBackground (CGContext context, CGRect rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
        {
            context.SaveState ();
            context.BeginTransparencyLayer (null);
            roundedRectanglePath.AddClip ();
            context.SetFillColor(fillColor);
            context.FillRect(rect);
            context.EndTransparencyLayer ();
            context.RestoreState ();
        }

        protected virtual void DrawStroke(CGColor fillColor, CGRect rect)
        {
            var biggerRect = Bounds.Copy().Grow(10);
            var holeRect = rect.Copy().Shrink(1);

            var roundedRectanglePath = UIBezierPath.FromRoundedRect (holeRect, _radiusCorner);

            var maskPath = new UIBezierPath();
            maskPath.MoveTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AppendPath(roundedRectanglePath);

            var maskWithHole = new CAShapeLayer();
            maskWithHole.Path = maskPath.CGPath;
            maskWithHole.FillRule = CAShapeLayer.FillRuleEvenOdd;

            _shadowView = new UIView(Frame);
            _shadowView.BackgroundColor = UIColor.White.ColorWithAlpha(0.7f);
            _shadowView.Layer.MasksToBounds = false;
            _shadowView.Layer.ShadowColor = UIColor.FromRGBA(0, 0, 0, 127).CGColor;
            _shadowView.Layer.ShadowOpacity = 1.0f;
            _shadowView.Layer.ShadowRadius = _radiusCorner + 1;
            _shadowView.Layer.ShadowOffset = new CGSize(0.3f, 0.3f);
            _shadowView.Layer.ShouldRasterize = true;     
            _shadowView.Layer.Mask = maskWithHole;
            _shadowView.Frame = Frame.Copy().Shrink(1);

            this.Superview.InsertSubviewBelow(_shadowView, this);    

            roundedRectanglePath.Dispose();
            maskPath.Dispose();
            maskWithHole.Display();
        }

        private void ClearShadowIfNecessary()
        {
            if (_shadowView != null)
            {
                _shadowView.RemoveFromSuperview();
                _shadowView = null;
            }
        }
    }
}

