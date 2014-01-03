using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	[Register ("PanielView")]
	public class PanelView : UIView
	{
		private UILabel _label;

	    public PanelView (IntPtr handle) : base(  handle )
		{
			Initialize();
		}

		private void Initialize()
		{
			_label = new UILabel( new RectangleF(0,0,Frame.Width,44) );
			_label.BackgroundColor = UIColor.Clear;
			_label.TextAlignment = UITextAlignment.Center;
			_label.TextColor = AppStyle.NavigationTitleColor;
			_label.Font = AppStyle.GetButtonFont( 20 );
			AddSubview( _label );
		}

		public string Title { 
			get { return _label.Text; }
			set { _label.Text = value;}
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var context = UIGraphics.GetCurrentContext();
			 
			var shadow = UIColor.FromRGBA(0,0,0,128).CGColor;
			var shadowOffset = new SizeF(2, -0);
			var shadowBlurRadius = 2.5f;

			var titleRectanglePath = UIBezierPath.FromRect(new RectangleF(0,0,rect.Width,44));

            AppStyle.NavigationBarColor.SetFill();
			titleRectanglePath.Fill();

			var rectanglePath = UIBezierPath.FromRect(rect);

			var rectangleBorderRect = rectanglePath.Bounds;
			rectangleBorderRect.Inflate(shadowBlurRadius, shadowBlurRadius);
			rectangleBorderRect.Offset(-shadowOffset.Width, -shadowOffset.Height);
			rectangleBorderRect = RectangleF.Union(rectangleBorderRect, rectanglePath.Bounds);
			rectangleBorderRect.Inflate(1, 1);
			
			var rectangleNegativePath = UIBezierPath.FromRect(rectangleBorderRect);
			rectangleNegativePath.AppendPath(rectanglePath);
			rectangleNegativePath.UsesEvenOddFillRule = true;
			
			context.SaveState();
			{
				var xOffset = shadowOffset.Width + (float)Math.Round(rectangleBorderRect.Width);
				var yOffset = shadowOffset.Height;
				context.SetShadowWithColor(
					new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
					shadowBlurRadius,
					shadow);
				
				rectanglePath.AddClip();
				var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(rectangleBorderRect.Width), 0);
				rectangleNegativePath.ApplyTransform(transform);
				UIColor.Gray.SetFill();
				rectangleNegativePath.Fill();
			}
			context.RestoreState();
		}
	}
}

