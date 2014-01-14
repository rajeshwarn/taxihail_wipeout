using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
		private float _radiusCorner = 2;

		public FlatTextField (IntPtr handle) : base (handle)
		{
			Initialize();
		}

		public FlatTextField (float radius) : base()
		{
			_radiusCorner = radius;
			Initialize();
		}

		public FlatTextField () : base()
		{
			Initialize();
		}

		public FlatTextField (RectangleF frame) : base (frame)
		{
			Initialize();
		}

		void Initialize ()
		{
			Height = 40;
			Font = UIFont.SystemFontOfSize(34/2);
			LeftView = new UIView(new RectangleF(0f,0f,13f,1f)); //left padding
			LeftViewMode = UITextFieldViewMode.Always;
		}

		public float Height
		{
			get;
			set;
		}

		public override void Draw (RectangleF rect)
		{           
			Frame = Frame.SetHeight(Height);

			this.BackgroundColor = UIColor.Clear;

			var context = UIGraphics.GetCurrentContext ();
			var fillColor = this.State.HasFlag (UIControlState.Normal)
			                ? UIColor.White 
			                : UIColor.Clear;

			var textColor = this.State.HasFlag (UIControlState.Selected)
			                ? UIColor.LightGray.CGColor
			                : UIColor.Black.CGColor;

			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _radiusCorner);

			DrawBackground(context, rect, roundedRectanglePath, fillColor.CGColor);
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

		private void DrawBackground (CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
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
			Layer.CornerRadius = _radiusCorner;
		}
	}
}

