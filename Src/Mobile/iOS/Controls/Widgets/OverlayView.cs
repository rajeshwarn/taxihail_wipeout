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
            DrawStroke(fillColor.CGColor);
        }

        void DrawBackground (CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
        {
            context.SaveState ();
            context.BeginTransparencyLayer (null);
            roundedRectanglePath.AddClip ();
            context.SetFillColorWithColor(fillColor);
            context.FillRect(rect);
            context.EndTransparencyLayer ();
            context.RestoreState ();
        }

        void DrawStroke(CGColor fillColor)
        {
            if (_shadowView != null)
            {
                _shadowView.RemoveFromSuperview();
            }
            _shadowView = new UIView(Frame);
            _shadowView.BackgroundColor = UIColor.White.ColorWithAlpha(0.7f);
            _shadowView.Layer.MasksToBounds = false;
            _shadowView.Layer.ShadowColor = UIColor.FromRGBA(0, 0, 0, 127).CGColor;
            _shadowView.Layer.ShadowOpacity = 1.0f;
            _shadowView.Layer.ShadowRadius = _radiusCorner+1;
            _shadowView.Layer.ShadowOffset = new SizeF(0.3f, 0.3f);
            _shadowView.Layer.ShouldRasterize = true;        
            _shadowView.Frame = Frame.Copy().Shrink(1);

            this.Superview.InsertSubviewBelow(_shadowView, this);            

        }
    }
}

