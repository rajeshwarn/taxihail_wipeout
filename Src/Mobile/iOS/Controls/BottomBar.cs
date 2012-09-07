using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BottomBar")]
	public class BottomBar : UIView
	{
		public BottomBar ()
		{
			Initialize();
		}

		public BottomBar (IntPtr handle) : base(  handle )
		{
			Initialize();
		}

		private void Initialize()
		{
			OutsideRect = new RectangleF(0,0,0,0);
		}


		public override void Draw (System.Drawing.RectangleF rect)
		{
			base.Draw (rect);

			//Fill
			var fillRectPath = UIBezierPath.FromRect( rect );
  			var backgroundColor = UIColor.Black.ColorWithAlpha( 0.2f );
			backgroundColor.SetFill();
			fillRectPath.Fill();

			//Stroke
			UIBezierPath strokePath = new UIBezierPath();
			strokePath.MoveTo( new PointF( rect.Left, rect.Top ) );
			strokePath.AddLineTo( new PointF( rect.Right, rect.Top ) );
			UIColor.Black.ColorWithAlpha( 0.35f ).SetStroke();
			strokePath.LineWidth = 1f;
			strokePath.Stroke();

		}

		public RectangleF OutsideRect { get; set; }

		public override bool PointInside (PointF point, UIEvent uievent)
		{
			return this.Frame.Contains( point) || OutsideRect.Contains( point );
		}
	}
}

