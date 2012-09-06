using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AnimatedArrowButton : VerticalButton
	{
		public AnimatedArrowButton ( RectangleF rect, UIColor gradientStart, UIColor gradientEnd ) : base( rect, gradientStart, gradientEnd )
		{
			SetCustomSelectedBackgroundColor( UIColor.FromRGB(53,136,204) );
			BackgroundColor = UIColor.Clear;
			Initialize();
		}

		public AnimatedArrowButton ( RectangleF rect, UIColor background ) : base( rect, background )
		{
			SetCustomSelectedBackgroundColor( UIColor.FromRGBA(0,0,0,0.5f) );
			Initialize();
		}

		private void Initialize()
		{
			ContentMode = UIViewContentMode.Center;
			SetImage( UIImage.FromFile( "Assets/VerticalButtonBar/rightArrow.png"), UIControlState.Normal );
			ImageView.BackgroundColor = UIColor.Clear;
		}

		public override void Animate (bool isOpen, float fullHeight)
		{
			UIView.BeginAnimations("Arrow");
			UIView.SetAnimationDuration(0.3);
			UIView.SetAnimationCurve( UIViewAnimationCurve.EaseIn );
			if( isOpen )
			{
				this.ImageView.Transform = CGAffineTransform.MakeRotation( 0f );
				Superview.Frame = new RectangleF( Superview.Frame.X, Superview.Frame.Y, Superview.Frame.Width, Frame.Height );
			}
			else
			{
				this.ImageView.Transform = CGAffineTransform.MakeRotation( (float)Math.PI/2 );
				Superview.Frame = new RectangleF( Superview.Frame.X, Superview.Frame.Y, Superview.Frame.Width, fullHeight );
			}
			UIView.CommitAnimations();
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
		}
	}
}

