using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
	    private const float RadiusCorner = 2;

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

		void Initialize ()
		{
			if(UIHelper.IsOS7orHigher)
			{
				TintColor = UIColor.Black; // cursor color
			}

			Font = UIFont.FromName("HelveticaNeue-Light", 38/2);
			LeftView = new UIView(new RectangleF(0f,0f,13f,1f)); //left padding
			LeftViewMode = UITextFieldViewMode.Always;
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

