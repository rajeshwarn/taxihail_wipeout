using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using apcurium.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AppButtons
	{
		public static GradientButton CreateStandardGradientButton ( RectangleF rect, string title, UIColor titleColor, AppStyle.ButtonColor buttonColor )
		{
			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, AppStyle.GetButtonColors(buttonColor), 
                                            AppStyle.GetButtonColorLocations( buttonColor ), AppStyle.ButtonStrokeLineWidth, AppStyle.GetButtonStrokeColor( buttonColor ),
                                            AppStyle.GetInnerShadow( buttonColor ), AppStyle.GetDropShadow( buttonColor ), title, titleColor, AppStyle.GetButtonFont( AppStyle.ButtonFontSize ) );

			return btn;
		}

        public static void FormatStandardGradientButton ( GradientButton button,  string title, UIColor titleColor, AppStyle.ButtonColor buttonColor )
        {

            button.TextShadowColor = UIColor.FromRGBA(0f, 0f, 0f, 0.5f);
            button.CornerRadius = AppStyle.ButtonCornerRadius;
            button.Colors=  AppStyle.GetButtonColors(buttonColor);
            button.ColorLocations = AppStyle.GetButtonColorLocations( buttonColor );
            button.StrokeLineWidth = AppStyle.ButtonStrokeLineWidth;
            button.StrokeLineColor = AppStyle.GetButtonStrokeColor(buttonColor );
            button.InnerShadow = AppStyle.GetInnerShadow( buttonColor );
            button.DropShadow = AppStyle.GetDropShadow( buttonColor );
            button.SetTitle( title , UIControlState.Normal );
            button.TitleColour = titleColor.CGColor;
            button.TitleFont = AppStyle.GetButtonFont( AppStyle.ButtonFontSize ) ;        
        }


		public static GradientButton CreateStandardButton ( RectangleF rect, string title, UIColor titleColor, AppStyle.ButtonColor buttonColor )
		{
			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, AppStyle.GetButtonColors( buttonColor ), AppStyle.GetButtonColorLocations( buttonColor ), AppStyle.ButtonStrokeLineWidth, AppStyle.GetButtonStrokeColor( buttonColor ), AppStyle.GetInnerShadow( buttonColor ), AppStyle.GetDropShadow( buttonColor ), title, titleColor, AppStyle.GetButtonFont( AppStyle.ButtonFontSize ) );

			return btn;
		}

		public static GradientButton CreateStandardImageButton ( RectangleF rect, string title, UIColor titleColor, string image, AppStyle.ButtonColor buttonColor )
		{
			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, AppStyle.GetButtonColors( buttonColor ), AppStyle.GetButtonColorLocations( buttonColor ), AppStyle.ButtonStrokeLineWidth, AppStyle.GetButtonStrokeColor( buttonColor ), AppStyle.GetInnerShadow( buttonColor ), AppStyle.GetDropShadow( buttonColor ), title, titleColor, AppStyle.GetButtonFont( AppStyle.ButtonFontSize ), image  );

			return btn;
		}

		public static UIView GetAccessoryView( string leftBtnTitle, UIColor leftBtnTitleColor, Action leftBtnAction, AppStyle.ButtonColor leftBtnColor, string rightBtnTitle, UIColor rightBtnTitleColor, Action rightBtnAction, AppStyle.ButtonColor rightBtnColor )
		{
			var v = new KeyboardAccessoryView( new RectangleF( 0, 0, 320, 36 ) );

			if( leftBtnTitle.HasValue() )
			{
				var leftBtn = AppButtons.CreateStandardGradientButton(new RectangleF( 5, 3, 46, 30 ), leftBtnTitle, leftBtnTitleColor, leftBtnColor );
				if( leftBtnAction != null )
				{
					leftBtn.TouchUpInside += delegate {
						leftBtnAction();
					};
				}
				v.AddSubview( leftBtn );
			}

			if( rightBtnTitle.HasValue() )
			{
				var rightBtn = AppButtons.CreateStandardGradientButton(new RectangleF( 269, 3, 46, 30 ), rightBtnTitle, rightBtnTitleColor, rightBtnColor );
				if( rightBtnAction != null )
				{
					rightBtn.TouchUpInside += delegate {
						rightBtnAction();
					};
				}
				v.AddSubview( rightBtn );
			}

			return v;
		}

		public static UIView GetAccessoryView( string btnTitle, UIColor btnTitleColor, Action btnAction, AppStyle.ButtonColor btnColor, ButtonPosition btnPosition )
		{
			var v = new KeyboardAccessoryView( new RectangleF( 0, 0, 320, 36 ) );

			if( btnTitle.HasValue() )
			{
				var x = 5f;
				var btnWidth = 46f;
				if( btnPosition == ButtonPosition.Right )
				{
					x = 320f - 5f - btnWidth;
				}
				var btn = AppButtons.CreateStandardGradientButton(new RectangleF( x, 3, btnWidth, 30 ), btnTitle, btnTitleColor, btnColor );
				if( btnAction != null )
				{
					btn.TouchUpInside += delegate {
						btnAction();
					};
				}
				v.AddSubview( btn );
			}

			return v;
		}
	
	}

	public enum ButtonPosition { Left, Right }

	public class KeyboardAccessoryView : UIView
	{
		public KeyboardAccessoryView( RectangleF rect ) : base( rect )
		{

		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var context = UIGraphics.GetCurrentContext();

			context.SetStrokeColor( UIColor.Black.CGColor );
			context.SaveState();
			context.BeginPath ();
			context.MoveTo( 0f, 0f );
			context.AddLineToPoint (320f, 0f);
			context.StrokePath();
			context.RestoreState();

			rect.Y = 1;
			var roundedRectanglePath = UIBezierPath.FromRect(rect);
			context.SaveState();
			roundedRectanglePath.AddClip();
			UIColor.FromRGB( 144, 152, 163 ).SetFill();
			roundedRectanglePath.Fill();
			context.RestoreState();

			context.SetStrokeColor( UIColor.FromRGBA( 255, 255, 255, 0.5f ).CGColor );
			context.SaveState();
			context.BeginPath ();
			context.MoveTo( 0f, 1f );
			context.AddLineToPoint (320f, 1f);
			context.StrokePath();
			context.RestoreState();


		}
	}
}

