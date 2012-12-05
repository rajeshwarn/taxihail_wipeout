using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("StatusBar")]
	public class StatusBar : UILabel
	{
		public StatusBar ()
		{
			Initialize ();
		}

		public StatusBar (IntPtr handle) : base(  handle )
		{
			Initialize();
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.FromPatternImage( UIImage.FromFile( "Assets/rideStatusBarBackground.png" ) );

			ClipsToBounds = false;

			Layer.MasksToBounds = false;

			Layer.ShadowColor = UIColor.Black.CGColor;
			Layer.ShadowOpacity = 0.65f;
			Layer.ShadowRadius = 1f;
			Layer.ShadowOffset  = new SizeF(0f, 0f);

			var size = Bounds.Size;
			var curlFactor = 2.0f;
			var shadowDepth = 3.0f;
			UIBezierPath path = new UIBezierPath();
			path.MoveTo( new PointF(-2f, size.Height - 1f));
			path.AddLineTo( new PointF(size.Width + 2f, size.Height -1f));
			path.AddLineTo( new PointF(size.Width + 2f, size.Height -1f  + shadowDepth));
			path.AddCurveToPoint( new PointF(-2f, size.Height -1f + shadowDepth),
			                     new PointF(size.Width + 2f - curlFactor, size.Height - 1f + shadowDepth - curlFactor),
			                     new PointF(curlFactor, size.Height - 1f + shadowDepth - curlFactor));

			Layer.ShadowPath = path.CGPath;
		}

		public override void Draw (System.Drawing.RectangleF rect)
		{

			base.Draw (rect);

			var rectpath = new UIBezierPath();
			rectpath.MoveTo( new PointF( 0,rect.Bottom ) );
			rectpath.AddLineTo( new PointF( rect.Right, rect.Bottom ) );

			UIColor.Black.SetStroke();
			rectpath.LineWidth = 1f;
			rectpath.Stroke();

		}
	}
}

