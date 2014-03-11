using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("OverlayButton")]
    public class OverlayButton : CommandButton
    {
        private float _radiusCorner = 2f;
        private UIView _shadowView = null;
		private UIColor fillColor;
		private UIColor fillColorNormal = UIColor.White.ColorWithAlpha(0.8f);
		private UIColor fillColorPressed = UIColor.Gray.ColorWithAlpha(0.8f);

        public OverlayButton(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;
			fillColor = fillColorNormal;

			this.TouchDown += (object sender, EventArgs e) => 
			{
				fillColor = fillColorPressed;
				this.SetNeedsDisplay();
			};				
        }

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			fillColor = fillColorNormal;
			this.SetNeedsDisplay();
		}

        public override void Draw (RectangleF rect)
        {           
			var context = UIGraphics.GetCurrentContext ();
            var textColor = UIColor.Black;

            var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _radiusCorner);

			DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor);
            DrawStroke(fillColor.CGColor);			

            SetNeedsDisplay();
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
            Layer.BorderWidth = 1.0f;
            Layer.BorderColor = fillColor;
            Layer.CornerRadius = _radiusCorner;

            if (_shadowView == null)
            {
                _shadowView = new UIView(Frame);
                _shadowView.BackgroundColor = UIColor.White.ColorWithAlpha(0.7f);
                _shadowView.Layer.MasksToBounds = false;
                _shadowView.Layer.ShadowColor = UIColor.FromRGBA(0, 0, 0, 127).CGColor;
                _shadowView.Layer.ShadowOpacity = 1.0f;
                _shadowView.Layer.ShadowRadius = _radiusCorner+1;
                _shadowView.Layer.ShadowOffset = new SizeF(0.3f, 0.3f);
                _shadowView.Layer.ShouldRasterize = true;        

                this.Superview.InsertSubviewBelow(_shadowView, this);
            }
            _shadowView.Frame = Frame.Copy().Shrink(1);
        }
    }
}

