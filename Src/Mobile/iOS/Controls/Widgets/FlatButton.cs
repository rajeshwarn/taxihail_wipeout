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
        private const float RadiusCorner = 2;
		private const float StandardImagePadding = 10f;
		private const float StandardImageWidth = 35f;

        public FlatButton (IntPtr handle) : base (handle)
        {
			ApplyDefaultStyle ();
        }

        public FlatButton (RectangleF frame) : base (frame)
        {
			ApplyDefaultStyle ();
        }
        public FlatButton ()
        {
			ApplyDefaultStyle ();
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

		private void ApplyDefaultStyle()
		{
			var blue = UIColor.FromRGB(0, 72, 129);

			Font = UIFont.FromName ("HelveticaNeue-Light", 40/2);

			SetFillColor(blue, UIControlState.Normal);
			SetFillColor(blue.ColorWithAlpha(0.5f), UIControlState.Selected);
			SetFillColor(blue.ColorWithAlpha(0.5f), UIControlState.Highlighted);

			SetTitleColor(UIColor.White, UIControlState.Normal);
			SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
			SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);

			SetStrokeColor(UIColor.Black, UIControlState.Normal);
			SetStrokeColor(UIColor.Black, UIControlState.Selected);
			SetStrokeColor(UIColor.Black, UIControlState.Highlighted);
		}

        public override void Draw (RectangleF rect)
		{
			// fix problem with ios6 using the default font
			if (Font.Name != FlatButtonStyle.ClearButtonFont.Name) {
				Font = UIFont.FromName ("HelveticaNeue-Light", 40/2);
			}

            var context = UIGraphics.GetCurrentContext ();

            var states = new [] { UIControlState.Disabled, UIControlState.Highlighted, UIControlState.Selected, UIControlState.Normal }
            .Where (x=> State.HasFlag(x)).Select(x=>(uint)x).ToArray();

            var fillColor = states.Where(x=> _fillColors.ContainsKey(x)).Select(x=> _fillColors[x]).FirstOrDefault() ?? UIColor.Clear;
            var strokeColor = states.Where(x=> _strokeColors.ContainsKey(x)).Select(x=> _strokeColors[x]).FirstOrDefault() ?? UIColor.Clear;

			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, RadiusCorner);

            DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor); 
            DrawStroke(context, roundedRectanglePath, strokeColor.CGColor);

            if (_leftImage != null)
            {
                SetImage(_leftImage, UIControlState.Normal);
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
				var leftPaddingForThisImage = StandardImagePadding + (StandardImageWidth - _leftImage.Size.Width) / 2; //calculate the left padding for this image to have images centered instead of left-aligned
				ImageEdgeInsets = new UIEdgeInsets(0.0f, leftPaddingForThisImage, 0.0f, 0.0f);
                //compute the left margin for the text and center it
                var halfTextSize = TitleLabel.Frame.Width / 2; 
				var center = (Frame.Width - _leftImage.Size.Width - StandardImagePadding - 3) / 2;
                TitleEdgeInsets = new UIEdgeInsets(0.0f, center - halfTextSize, 0.0f, 0.0f);
            }

			SetNeedsDisplay();
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

        void DrawStroke (CGContext context, UIBezierPath roundedRectanglePath, CGColor strokeColor)
        {
            context.SaveState ();
            context.SetStrokeColorWithColor(strokeColor);
            roundedRectanglePath.AddClip ();
            context.AddPath (roundedRectanglePath.CGPath);
            context.StrokePath();
            context.RestoreState ();
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

