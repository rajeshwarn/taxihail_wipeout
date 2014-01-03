using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
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


		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			//Fill
			var fillRectPath = UIBezierPath.FromRect( rect );
  			var backgroundColor = UIColor.Black.ColorWithAlpha( 0.2f );
			backgroundColor.SetFill();
			fillRectPath.Fill();

			//Stroke
			var strokePath = new UIBezierPath();
			strokePath.MoveTo( new PointF( rect.Left, rect.Top ) );
			strokePath.AddLineTo( new PointF( rect.Right, rect.Top ) );
			UIColor.Black.ColorWithAlpha( 0.35f ).SetStroke();
			strokePath.LineWidth = 1f;
			strokePath.Stroke();

		}

// ReSharper disable once UnusedAutoPropertyAccessor.Local
		public RectangleF OutsideRect { private get; set; }
	}
}

