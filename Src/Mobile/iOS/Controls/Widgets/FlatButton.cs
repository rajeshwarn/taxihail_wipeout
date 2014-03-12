using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatButton")]
    public class FlatButton : CommandButton
    {
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

        public FlatButton () : base()
        {
			ApplyDefaultStyle ();
        }
		
        public void SetFillColor (UIColor color, UIControlState state)
        {
			this.SetBackgroundImage(GetImage(color), state);
        }

        UIImage GetImage(UIColor color)
        {
            var rect = new RectangleF(0.0f, 0.0f, 1.0f, 1.0f);
            UIGraphics.BeginImageContext(rect.Size);
            var context = UIGraphics.GetCurrentContext();

            context.SetFillColorWithColor(color.CGColor);
            context.FillRect(rect);

            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }
		
        public void SetStrokeColor (UIColor color)
        {
            this.Layer.BorderWidth = UIHelper.OnePixel;
            this.Layer.BorderColor = color.CGColor;
        }

		private void ApplyDefaultStyle()
        {
            Font = UIFont.FromName(FontName.HelveticaNeueRegular, 40 / 2);

            SetFillColor(UIColor.Clear, UIControlState.Normal);
            SetFillColor(UIColor.Clear, UIControlState.Selected);
            SetFillColor(UIColor.Clear, UIControlState.Highlighted);

            SetTitleColor(Theme.ButtonTextColor, UIControlState.Normal);
            SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Selected);
            SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);

            SetStrokeColor(UIColor.FromRGB(3, 27, 49));

            this.Layer.CornerRadius = RadiusCorner;
            this.Layer.MasksToBounds = true;
		}

        private NSLayoutConstraint[] _hiddenContraints { get; set; }

        public bool HiddenWithConstraints
        {
            get
            {
                return base.Hidden;
            }
            set
            {
                if (base.Hidden != value)
                {
                    base.Hidden = value;
                    if (value)
                    {
                        _hiddenContraints = this.Superview.Constraints != null 
                                            ? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
                                            : null;
                        if (_hiddenContraints != null)
                        {
                            this.Superview.RemoveConstraints(_hiddenContraints);
                        }
                    }
                    else
                    {
                        if (_hiddenContraints != null)
                        {
                            this.Superview.AddConstraints(_hiddenContraints);
                            _hiddenContraints = null;
                        }
                    }
                }
            }
        }
        
        public void SetLeftImage( string image )
        {
            if (image != null)
            {
                var leftImage = UIImage.FromFile(image);
                SetImage(leftImage, UIControlState.Normal);
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left;

                //calculate the left padding for this image to have images centered instead of left-aligned
                var leftPaddingForThisImage = StandardImagePadding + (StandardImageWidth - leftImage.Size.Width) / 2; 
                ImageEdgeInsets = new UIEdgeInsets(0.0f, leftPaddingForThisImage, 0.0f, 0.0f);

                //compute the left margin for the text and center it
                var halfTextSize = TitleLabel.Frame.Width / 2; 
                var center = (Frame.Width - leftImage.Size.Width - StandardImagePadding - 3) / 2;
                TitleEdgeInsets = new UIEdgeInsets(0.0f, center - halfTextSize, 0.0f, 0.0f);

                SetNeedsDisplay();
            }
        }
    }
}

