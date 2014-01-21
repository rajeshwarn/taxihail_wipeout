using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
	    private const float RadiusCorner = 2;
        private const float Padding = 13f;

	    public FlatTextField (IntPtr handle) : base (handle)
		{
			Initialize();
		}

		public FlatTextField ()
		{
			Initialize();
		}

		public FlatTextField (RectangleF frame) : base (frame)
		{
			Initialize();
		}

        private void Initialize ()
		{
            if(UIHelper.IsOS7orHigher)
			{
				TintColor = UIColor.Black; // cursor color
			}

			Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);

            //padding
            LeftView = new UIView(new RectangleF(0f, 0f, Padding, 1f)); 
			LeftViewMode = UITextFieldViewMode.Always;
            RightView = new UIView(new RectangleF(Frame.Right - Padding, 0f, Padding, 1f));
            RightViewMode = UITextFieldViewMode.Always;
		}

		public override void Draw (RectangleF rect)
		{   
            var fillColor = State.HasFlag (UIControlState.Normal)
			                ? UIColor.White 
			                : UIColor.Clear;

			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, RadiusCorner);

			DrawBackground(UIGraphics.GetCurrentContext(), rect, roundedRectanglePath, fillColor.CGColor);
			DrawStroke(fillColor.CGColor);
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

        private bool _hasRightArrow { get; set; }
        public bool HasRightArrow {
            get {
                return _hasRightArrow;
            }
            set {
                _hasRightArrow = value;

                if(value)
                {
                    var image = UIImage.FromFile("right_arrow.png");
                    var rightArrow = new UIImageView(new RectangleF(Frame.Width - image.Size.Width - Padding, (Frame.Height - image.Size.Height)/2, image.Size.Width, image.Size.Height));
                    rightArrow.Image = image;

                    RightView.Frame = RightView.Frame.IncrementWidth(image.Size.Width);
                    AddSubview(rightArrow);

                    SetNeedsDisplay();
                }
            }
        }

		private void DrawBackground(CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
		{
			context.SaveState ();
			context.BeginTransparencyLayer (null);
			roundedRectanglePath.AddClip ();
			context.SetFillColorWithColor(fillColor);
			context.FillRect(rect);
			context.EndTransparencyLayer ();
			context.RestoreState ();
		}

		private void DrawStroke(CGColor fillColor)
		{
			BorderStyle = UITextBorderStyle.None;
			Layer.BorderWidth = 1.0f;
			Layer.BorderColor = fillColor;
			Layer.CornerRadius = RadiusCorner;
		}
	}
}

