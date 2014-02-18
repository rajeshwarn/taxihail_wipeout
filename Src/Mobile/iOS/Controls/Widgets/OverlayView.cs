using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MonoTouch.CoreAnimation;
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

        public OverlayView(RectangleF frame) : base(frame)
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

        public override void Draw (RectangleF rect)
        {           
            var context = UIGraphics.GetCurrentContext ();

            var fillColor = UIColor.White.ColorWithAlpha(0.8f);

            var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _radiusCorner);

            DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor);
            DrawStroke(fillColor.CGColor, roundedRectanglePath);
        }

        protected virtual void DrawBackground (CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
        {
            context.SaveState ();
            context.BeginTransparencyLayer (null);
            roundedRectanglePath.AddClip ();
            context.SetFillColorWithColor(fillColor);
            context.FillRect(rect);
            context.EndTransparencyLayer ();
            context.RestoreState ();
        }

        protected virtual void DrawStroke(CGColor fillColor, UIBezierPath roundedRectanglePath)
        {
            if (_shadowView != null)
            {
                _shadowView.RemoveFromSuperview();
            }

            var expandedFrame = Bounds.Copy().Shrink(-10);

            var maskWithHole = new CAShapeLayer();

            // Both frames are defined in the same coordinate system
            var biggerRect = expandedFrame;
            var smallerRect = Bounds;

            var maskPath = new UIBezierPath();
            maskPath.MoveTo(new PointF(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new PointF(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new PointF(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new PointF(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new PointF(biggerRect.GetMinX(), biggerRect.GetMinY()));

            maskPath.MoveTo(new PointF(smallerRect.GetMinX(), smallerRect.GetMinY()));
            maskPath.AddLineTo(new PointF(smallerRect.GetMinX(), smallerRect.GetMaxY()));
            maskPath.AddLineTo(new PointF(smallerRect.GetMaxX(), smallerRect.GetMaxY()));
            maskPath.AddLineTo(new PointF(smallerRect.GetMaxX(), smallerRect.GetMinY()));
            maskPath.AddLineTo(new PointF(smallerRect.GetMinX(), smallerRect.GetMinY()));

            maskWithHole.Path = maskPath.CGPath;
            maskWithHole.FillRule = CAShapeLayer.FillRuleEvenOdd;

            _shadowView = new UIView(Frame);
            _shadowView.BackgroundColor = UIColor.White.ColorWithAlpha(0.7f);
            _shadowView.Layer.MasksToBounds = false;
            _shadowView.Layer.ShadowColor = UIColor.FromRGBA(0, 0, 0, 127).CGColor;
            _shadowView.Layer.ShadowOpacity = 1.0f;
            _shadowView.Layer.ShadowRadius = _radiusCorner + 1;
            _shadowView.Layer.ShadowOffset = new SizeF(0.3f, 0.3f);
            _shadowView.Layer.ShouldRasterize = true;     
            _shadowView.Layer.Mask = maskWithHole;
            _shadowView.Frame = Frame.Copy().Shrink(1);

            this.Superview.InsertSubviewBelow(_shadowView, this);            
        }
    }
}

