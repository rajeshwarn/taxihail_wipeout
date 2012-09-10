using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Common.Extensions;
using MonoTouch.CoreGraphics;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AddressBarButtons : UIView
	{
		public AddressButton BtnAddress;
		private UIButton _btnCurrentPosition;
		private UIButton _btnActivate;

		public AddressBarButtons ( RectangleF rect ) : base( rect )
		{
			Initialize();
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.Clear;

			_btnActivate = new UIButton( new RectangleF( Frame.Width - 40, 0, 40, Frame.Height ) );
			_btnActivate.BackgroundColor = UIColor.Clear;
			_btnActivate.SetImage( UIImage.FromFile( "Assets/VerticalButtonBar/targetIcon.png" ), UIControlState.Normal );
			this.AddSubview( _btnActivate );

			_btnActivate.TouchUpInside += HandleActivateTouchUpInside;
		
			_btnCurrentPosition = new UIButton( new RectangleF( Frame.Width - 80, 0, 40, Frame.Height ) );
			_btnCurrentPosition.BackgroundColor = UIColor.Clear;
			_btnCurrentPosition.SetImage( UIImage.FromFile( "Assets/VerticalButtonBar/locationIcon.png" ), UIControlState.Normal );
			this.AddSubview( _btnCurrentPosition );

			_btnCurrentPosition.TouchUpInside += HandleCurrentPositionTouchUpInside;

			BtnAddress = new AddressButton( new RectangleF( 0, 0, Frame.Width - 80, Frame.Height ) );
			BtnAddress.BackgroundColor = UIColor.Clear;
			this.AddSubview( BtnAddress );

			_btnCurrentPosition.TouchUpInside += HandleCurrentPositionTouchUpInside;
		}

		void HandleActivateTouchUpInside (object sender, EventArgs e)
		{
			_btnActivate.Highlighted = !_btnActivate.Highlighted;
		}

		void HandleCurrentPositionTouchUpInside (object sender, EventArgs e)
		{

		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var context = UIGraphics.GetCurrentContext();
			
			var newGradientColors = AppStyle.GetButtonColors( AppStyle.ButtonColor.Silver ).Select( c => c.CGColor ).ToArray();
			var newGradientLocations = AppStyle.GetButtonColorLocations( AppStyle.ButtonColor.Silver );
			var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);

			var radius = 0;
//
//			ShadowSetting dropShadow = null;
//			var innerShadow = AppStyle.GetInnerShadow( AppStyle.ButtonColor.Silver );
//
//			rect.Width -= dropShadow != null ? Math.Abs(dropShadow.Offset.Width) : 0;
//			rect.Height -= dropShadow != null ? Math.Abs(dropShadow.Offset.Height) : 0;
//			rect.X += dropShadow != null && dropShadow.Offset.Width < 0 ? Math.Abs(dropShadow.Offset.Width) : 0;
//			rect.Y += dropShadow != null && dropShadow.Offset.Height < 0 ? Math.Abs(dropShadow.Offset.Height) : 0;


			var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, radius);

			context.SaveState();
//			if (dropShadow != null)
//			{
//				context.SetShadowWithColor(dropShadow.Offset, dropShadow.BlurRadius, dropShadow.Color.CGColor);
//			}
			
			context.BeginTransparencyLayer(null);
			roundedRectanglePath.AddClip();
			context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
			context.EndTransparencyLayer();
			context.RestoreState();

//			if (innerShadow != null)
//			{
//				var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
//				roundedRectangleBorderRect.Inflate(innerShadow.BlurRadius, innerShadow.BlurRadius);
//				roundedRectangleBorderRect.Offset(-innerShadow.Offset.Width, -innerShadow.Offset.Height);
//				roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
//				roundedRectangleBorderRect.Inflate(1, 1);
//				
//				var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
//				roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
//				roundedRectangleNegativePath.UsesEvenOddFillRule = true;
//				
//				context.SaveState();
//				{
//					var xOffset = innerShadow.Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
//					var yOffset = innerShadow.Offset.Height;
//					context.SetShadowWithColor(
//						new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
//						innerShadow.BlurRadius,
//						innerShadow.Color.CGColor);
//					
//					roundedRectanglePath.AddClip();
//					var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
//					roundedRectangleNegativePath.ApplyTransform(transform);
//					UIColor.Gray.SetFill();
//					roundedRectangleNegativePath.Fill();
//				}
//				context.RestoreState();
//			}

			


		}
	}
}

