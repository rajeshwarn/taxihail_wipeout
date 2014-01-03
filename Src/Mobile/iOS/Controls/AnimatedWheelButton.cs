using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public sealed class AnimatedWheelButton : VerticalButton
	{
		public AnimatedWheelButton ( RectangleF rect, UIColor gradientStart, UIColor gradientEnd ) : base( rect, gradientStart, gradientEnd )
		{
			SetCustomSelectedBackgroundColor( UIColor.FromRGB(53,136,204) );
			BackgroundColor = UIColor.Clear;
			Initialize();
		}
		
		public AnimatedWheelButton ( RectangleF rect, UIColor background ) : base( rect, background )
		{
			SetCustomSelectedBackgroundColor( UIColor.FromRGBA(0,0,0,0.5f) );
			Initialize();
		}
		
		private void Initialize()
		{
			ContentMode = UIViewContentMode.Center;
			SetImage( UIImage.FromFile( "Assets/VerticalButtonBar/wheel.png"), UIControlState.Normal );
			ImageView.BackgroundColor = UIColor.Clear;
		}

		public override void Animate (bool isOpen, float fullHeight)
		{
			base.Animate( isOpen, fullHeight );

			BeginAnimations("Wheel");
			SetAnimationDuration(0.3);
			SetAnimationCurve( UIViewAnimationCurve.EaseIn );
			if( isOpen )
			{
				ImageView.Transform = CGAffineTransform.MakeRotation( 0f );
			}
			else
			{
				ImageView.Transform = CGAffineTransform.MakeRotation( (float)Math.PI );
			}
			CommitAnimations();
		}
	}
}

