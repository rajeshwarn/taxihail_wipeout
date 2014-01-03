using System;
using System.Linq;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.UIKit;
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

        public static void FormatStandardButton (GradientButton button, string title, AppStyle.ButtonColor buttonColor, string image = null, string rightImage = null, UIControlContentHorizontalAlignment horizontalAlignment = UIControlContentHorizontalAlignment.Center)
        {
            var btnStyle = StyleManager.Current.Buttons.Single (b => b.Key == buttonColor.ToString ());

            btnStyle.TextShadowColor.Maybe (c=> button.TextShadowColor = UIColor.FromRGBA (c.Red, c.Green, c.Blue, c.Alpha));
            btnStyle.SelectedTextShadowColor.Maybe (c=> button.SelectedTextShadowColor = UIColor.FromRGBA (c.Red, c.Green, c.Blue, c.Alpha));
            button.CornerRadius = AppStyle.ButtonCornerRadius;
            button.Colors =  btnStyle.Colors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
			btnStyle.SelectedColors.Maybe( () => {
				button.SelectedColors =  btnStyle.SelectedColors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
				button.SelectedColorLocations = btnStyle.SelectedColors.Select ( color => color.Location ).ToArray();
			});
            button.ColorLocations = btnStyle.Colors.Select ( color => color.Location ).ToArray();
			
			button.StrokeLineWidth = btnStyle.StrokeLineWidth;
			button.StrokeLineColor = UIColor.FromRGBA( btnStyle.StrokeColor.Red, btnStyle.StrokeColor.Green, btnStyle.StrokeColor.Blue, btnStyle.StrokeColor.Alpha );
            btnStyle.SelectedStrokeColor.Maybe(c=> button.SelectedStrokeLineColor = UIColor.FromRGBA(c.Red, c.Green, c.Blue, c.Alpha));
			button.InnerShadow = btnStyle.InnerShadow;
            button.SetImage( image );
            button.SetRightImage( rightImage );
            button.ContentEdgeInsets = new UIEdgeInsets(0, 3, 0, 10);
			button.DropShadow = btnStyle.DropShadow;
            button.HorizontalAlignment = horizontalAlignment;
            button.SetTitle( title , UIControlState.Normal );
            btnStyle.TextColor.Maybe( c => button.TitleColour = UIColor.FromRGBA( c.Red, c.Green, c.Blue, c.Alpha ).CGColor );
			btnStyle.SelectedTextColor.Maybe( c => button.SelectedTitleColour = UIColor.FromRGBA( c.Red, c.Green, c.Blue, c.Alpha ).CGColor );
            button.TitleFont = AppStyle.ButtonFont;
             
        }

		public static UIView GetAccessoryView( string leftBtnTitle, UIColor leftBtnTitleColor, Action leftBtnAction, AppStyle.ButtonColor leftBtnColor, string rightBtnTitle, UIColor rightBtnTitleColor, Action rightBtnAction, AppStyle.ButtonColor rightBtnColor )
		{
			var v = new KeyboardAccessoryView( new RectangleF( 0, 0, 320, 36 ) );

			if( leftBtnTitle.HasValue() )
			{
				var leftBtn = CreateStandardButton(new RectangleF( 5, 3, 46, 30 ), leftBtnTitle, leftBtnColor );
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
				var rightBtn = CreateStandardButton(new RectangleF( 269, 3, 46, 30 ), rightBtnTitle, rightBtnColor );
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
				var btn = CreateStandardButton(new RectangleF( x, 3, btnWidth, 30 ), btnTitle, btnColor );
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

