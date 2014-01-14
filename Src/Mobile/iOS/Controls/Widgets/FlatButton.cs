using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;



namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatButton")]
    public class FlatButton : UIButton
    {
        UIImage _leftImage;
		private float _radiusCorner = 2;

        public FlatButton (IntPtr handle) : base (handle)
        {
			ApplyTo ();
        }

        public FlatButton (RectangleF frame) : base (frame)
        {
			ApplyTo ();
        }
        public FlatButton () : base ()
        {
			ApplyTo ();
        }


		IDictionary<uint, UIColor> _fillColors = new Dictionary<uint, UIColor>();
        public void SetFillColor (UIColor color, UIControlState state)
        {
			_fillColors[(uint)state] = color;
        }

		IDictionary<uint, UIColor> _strokeColors = new Dictionary<uint, UIColor>();
        public void SetStrokeColor (UIColor color, UIControlState state)
        {
			_strokeColors[(uint)state] = color;

        }

		private void ApplyTo()
		{
			UIColor Blue = UIColor.FromRGB(0, 71, 133);

			SetFillColor(Blue, UIControlState.Normal);
			SetFillColor(Blue.ColorWithAlpha(0.5f), UIControlState.Selected);
			SetFillColor(Blue.ColorWithAlpha(0.5f), UIControlState.Highlighted);

			SetTitleColor(UIColor.White, UIControlState.Normal);
			SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
			SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);

			SetStrokeColor(UIColor.Black, UIControlState.Normal);
			SetStrokeColor(UIColor.Black, UIControlState.Selected);
			SetStrokeColor(UIColor.Black, UIControlState.Highlighted);
		}

        public override void Draw (RectangleF rect)
        {
            var context = UIGraphics.GetCurrentContext ();

            var states = new [] { UIControlState.Disabled, UIControlState.Highlighted, UIControlState.Selected, UIControlState.Normal }
            .Where (x=> this.State.HasFlag(x)).Select(x=>(uint)x).ToArray();

            var fillColor = states.Where(x=> _fillColors.ContainsKey(x)).Select(x=> _fillColors[x]).FirstOrDefault() ?? UIColor.Clear;
            var strokeColor = states.Where(x=> _strokeColors.ContainsKey(x)).Select(x=> _strokeColors[x]).FirstOrDefault() ?? UIColor.Clear;
            var textColor = TitleColor(State) ?? UIColor.White;

			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _radiusCorner);

            DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor); 
            DrawStroke(context, rect, roundedRectanglePath, strokeColor.CGColor);

            if (_leftImage != null)
            {
                this.SetImage(_leftImage, UIControlState.Normal);
                this.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                this.ImageEdgeInsets = new UIEdgeInsets(0.0f, 10.0f, 0.0f, 0.0f);
                //compute the left margin for the text and center it
                var halfTextSize = this.TitleLabel.Frame.Width / 2; 
                var center = (this.Frame.Width - _leftImage.Size.Width - 10 - 3) / 2;
                this.TitleEdgeInsets = new UIEdgeInsets(0.0f, center - halfTextSize, 0.0f, 0.0f);
            }

            DrawText(context, rect, textColor.CGColor);

        }

        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled = value;
                SetNeedsDisplay();
            }
        }

        public override bool Highlighted {
            get {
                return base.Highlighted;
            }
            set {
                base.Highlighted = value;
                SetNeedsDisplay();
            }
        }

        public override bool Selected {
            get {
                return base.Selected;
            }
            set {
                base.Selected = value;
                SetNeedsDisplay();
            }
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

        void DrawStroke (CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor strokeColor)
        {
            context.SaveState ();
            context.SetStrokeColorWithColor(strokeColor);
            roundedRectanglePath.AddClip ();
            context.AddPath (roundedRectanglePath.CGPath);
            context.StrokePath();
            context.RestoreState ();
        }

        protected virtual void DrawText(CGContext context, RectangleF rect, CGColor textColor)
        {

        }

        public void SetLeftImage( string image )
        {
            if (image != null)
            {
                _leftImage = UIImage.FromFile(image);
                SetNeedsDisplay();
            }
        }
    }
}

