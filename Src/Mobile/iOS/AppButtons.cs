using System;
using System.Linq;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Style;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AppButtons
	{
		public static GradientButton CreateStandardButton ( RectangleF rect, string title, AppStyle.ButtonColor buttonColor, string image = null )
		{
			var btnStyle = StyleManager.Current.Buttons.Single( b => b.Key == buttonColor.ToString() );

			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, btnStyle, title, AppStyle.ButtonFont, image );

			return btn;
		}

        public static void FormatStandardButton ( GradientButton button,  string title, AppStyle.ButtonColor buttonColor, string image = null  )
        {
			var btnStyle = StyleManager.Current.Buttons.Single( b => b.Key == buttonColor.ToString() );

            button.TextShadowColor = UIColor.FromRGBA(btnStyle.TextShadowColor.Red, btnStyle.TextShadowColor.Green, btnStyle.TextShadowColor.Blue, btnStyle.TextShadowColor.Alpha);
            button.CornerRadius = AppStyle.ButtonCornerRadius;
            button.Colors =  btnStyle.Colors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
			btnStyle.SelectedColors.Maybe( () => {
				button.SelectedColors =  btnStyle.SelectedColors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
				button.SelectedColorLocations = btnStyle.SelectedColors.Select ( color => color.Location ).ToArray();
			});
            button.ColorLocations = btnStyle.Colors.Select ( color => color.Location ).ToArray();
			
			button.StrokeLineWidth = btnStyle.StrokeLineWidth;
			button.StrokeLineColor = UIColor.FromRGBA( btnStyle.StrokeColor.Red, btnStyle.StrokeColor.Green, btnStyle.StrokeColor.Blue, btnStyle.StrokeColor.Alpha );
			button.InnerShadow = btnStyle.InnerShadow;
            button.SetImage( image );
			button.DropShadow = btnStyle.DropShadow;
            button.SetTitle( title , UIControlState.Normal );
			btnStyle.TextColor.Maybe( c => button.TitleColour = UIColor.FromRGBA( c.Red, c.Green, c.Blue, c.Alpha ).CGColor );
            button.TitleFont = AppStyle.ButtonFont;        
        }


//		public static GradientButton CreateStandardsButton ( RectangleF rect, string title, UIColor titleColor, AppStyle.ButtonColor buttonColor,  bool useTitleShadow = true )
//		{
//			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, AppStyle.GetButtonColors( buttonColor ), AppStyle.GetButtonColorLocations( buttonColor ), AppStyle.ButtonStrokeLineWidth, AppStyle.GetButtonStrokeColor( buttonColor ), AppStyle.GetInnerShadow( buttonColor ), AppStyle.GetDropShadow( buttonColor ), title, titleColor, AppStyle.GetButtonFont( AppStyle.ButtonFontSize ), useTitleShadow );
//
//			return btn;
//		}

//		public static GradientButton CreateStandardButton ( RectangleF rect, string title, string image, AppStyle.ButtonColor buttonColor )
//		{( 
//			var btnStyle = StyleManager.Current.Buttons.Single( b => b.Key == buttonColor.ToString() );
//		
//			var btn = new GradientButton( rect, AppStyle.ButtonCornerRadius, btnStyle, title, AppStyle.ButtonFont, image  );
//
//			return btn;
//		}

		public static UIView GetAccessoryView( string leftBtnTitle, UIColor leftBtnTitleColor, Action leftBtnAction, AppStyle.ButtonColor leftBtnColor, string rightBtnTitle, UIColor rightBtnTitleColor, Action rightBtnAction, AppStyle.ButtonColor rightBtnColor )
		{
			var v = new KeyboardAccessoryView( new RectangleF( 0, 0, 320, 36 ) );

			if( leftBtnTitle.HasValue() )
			{
				var leftBtn = AppButtons.CreateStandardButton(new RectangleF( 5, 3, 46, 30 ), leftBtnTitle, leftBtnColor );
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
				var rightBtn = AppButtons.CreateStandardButton(new RectangleF( 269, 3, 46, 30 ), rightBtnTitle, rightBtnColor );
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
				var btn = AppButtons.CreateStandardButton(new RectangleF( x, 3, btnWidth, 30 ), btnTitle, btnColor );
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

