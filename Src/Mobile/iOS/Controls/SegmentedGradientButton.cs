using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Style;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("SegmentedGradientButton")]
    public class SegmentedGradientButton: GradientButton
    {
        public SegmentedGradientButton(IntPtr handle): base(handle)
        {

        }

        public SegmentedGradientButton (RectangleF rect): base(rect)
        {

        }

        private SegmentedGradientButton (RectangleF rect, float cornerRadius, ButtonStyle buttonStyle, string title, UIFont titleFont, string image)
            : base(rect, cornerRadius, buttonStyle, title, titleFont, image)
        {

        }

        private bool _isLeftButton;
        public bool IsLeftButton {
            get {
                return _isLeftButton;
            }
            set {
                if(_isLeftButton != value)
                {
                    _isLeftButton = value;
                    SetNeedsDisplay();
                }
            }
        }

        protected override void DrawStroke (MonoTouch.CoreGraphics.CGContext context, UIBezierPath roundedRectanglePath, RectangleF rect)
        {
            if (Selected) {
                base.DrawStroke (context, roundedRectanglePath, rect);
            } else {
                context.SaveState ();
                base.StrokeLineColor.SetStroke ();

                // Rect with radius, will be used to clip the entire view
                float minx = rect.GetMinX () + 1, midx = rect.GetMidX (), maxx = rect.GetMaxX ();
                float miny = rect.GetMinY () + 1, midy = rect.GetMidY (), maxy = rect.GetMaxY ();
            
            
                // Path are drawn starting from the middle of a pixel, in order to avoid an antialiased line
                if (IsLeftButton) {
                    context.MoveTo (maxx - .5f, miny - .5f);
                    context.AddLineToPoint (midx - .5f, miny - .5f);
                    context.AddArcToPoint (minx - .5f, miny - .5f, minx - .5f, midy - .5f, base.CornerRadius);
                    context.AddArcToPoint (minx - .5f, maxy - .5f, midx - .5f, maxy - .5f, base.CornerRadius);
                    context.AddLineToPoint (maxx - .5f, maxy - .5f);
                } else {
                    context.MoveTo (minx - .5f, miny - .5f);
                    context.AddLineToPoint (midx - .5f, miny - .5f);
                    context.AddArcToPoint (maxx - .5f, miny - .5f, maxx - .5f, midy - .5f, base.CornerRadius);
                    context.AddArcToPoint (maxx - .5f, maxy - .5f, midx - .5f, maxy - .5f, base.CornerRadius);
                    context.AddLineToPoint (minx - .5f, maxy - .5f);
                }

                context.StrokePath ();
                context.RestoreState ();
            }
        }
    }
}

