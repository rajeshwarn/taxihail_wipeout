using System.Linq;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.Framework.Extensions;
using System;
using MonoTouch.Foundation;
using MonoTouch.CoreText;

namespace apcurium.MK.Booking.Mobile.Client
{
    public static class AppStyle
    {
        public enum ButtonColor
        {
            Black,
            Grey,
            Green,
            Red,
            CorporateColor,
            Silver,
            AlternateCorporateColor,
            DarkGray,   
            Blue,
            LightBlue,
            SegmentedLightBlue,
        };

    
        public static float ButtonStrokeLineWidth { get { return 2f; } }

        public static float ButtonCornerRadius { get { return StyleManager.Current.ButtonCornerRadius.HasValue ? StyleManager.Current.ButtonCornerRadius.Value : 2f; } }

        public static UIColor GreyText { get { return UIColor.FromRGB (86, 86, 86); } }

        public static UIColor LightGreyText { get { return UIColor.FromRGB (150, 150, 150); } }

        public static UIColor NavigationTitleColor { get { return  UIColor.FromRGBA (StyleManager.Current.NavigationTitleColor.Red, StyleManager.Current.NavigationTitleColor.Green, StyleManager.Current.NavigationTitleColor.Blue, StyleManager.Current.NavigationTitleColor.Alpha); } }

        public static UIColor NavigationBarColor { get { return  UIColor.FromRGBA (StyleManager.Current.NavigationBarColor.Red, StyleManager.Current.NavigationBarColor.Green, StyleManager.Current.NavigationBarColor.Blue, StyleManager.Current.NavigationBarColor.Alpha); } }

        public static UIColor LightCorporateColor { get { return  UIColor.FromRGBA (StyleManager.Current.LightCorporateTextColor.Red, StyleManager.Current.LightCorporateTextColor.Green, StyleManager.Current.LightCorporateTextColor.Blue, StyleManager.Current.LightCorporateTextColor.Alpha); } }

        public static UIColor DarkText { get { return UIColor.FromRGB (50, 50, 50); } }

        public static UIColor TitleTextColor { get { return UIColor.FromRGB (66, 63, 58); } }

        public static UIFont NormalTextFont { get { return UIFont.FromName (RegularFontName, CellFontSize); } }

        public static UIFont BoldTextFont { get {         return UIFont.FromName (BoldFontName, CellFontSize);   } }

        public static string ButtonFontName { get { return BoldFontName; } }                

        public static string BoldFontName { get { 
                if (StyleManager.Current.UseCustomFonts) {
                    return StyleManager.Current.CustomBoldFont;
                } else {
                    return "HelveticaNeue-Bold"; 
                }
            } }

        
        public static string RegularFontName { get { 
                if (StyleManager.Current.UseCustomFonts) {

                    return StyleManager.Current.CustomRegularFont;
                } else {
                    return "HelveticaNeue"; 
                }
            } }

        public static string ItalicFontName { get { 
                if (StyleManager.Current.UseCustomFonts) {
                    return StyleManager.Current.CustomItalicFont;
                } else {
                    return "HelveticaNeue-Italic"; 
                }
            } }


        public static UIFont CellFont { get { return UIFont.FromName (BoldFontName, CellFontSize); } }

        public static UIFont CellSmallFont { get { return UIFont.FromName (BoldFontName, CellSmallFontSize); } }

        public static float CellFontSize { get { return 16f; } }

        public static UIFont ButtonFont { get { return GetButtonFont (ButtonFontSize); } }

        public static float CellSmallFontSize { get { return 12f; } }

        public static float ButtonFontSize { get { return StyleManager.Current.ButtonFontSize.HasValue ? StyleManager.Current.ButtonFontSize.Value : 14f; } }

        public static UIFont GetButtonFont (float size)
        {
            return UIFont.FromName (ButtonFontName, size);
        }

        public static UIFont GetNormalFont (float size)
        {
           
            return UIFont.FromName (RegularFontName, size);
        }

        public static UIFont GetBoldFont (float size)
        {
            return UIFont.FromName (BoldFontName, size);
        }

        public static UIFont GetItalicFont (float size)
        {
            return UIFont.FromName (ItalicFontName, size);
        }

        public  static  UIColor AcceptButtonColor{ get { return UIColor.FromRGB (19, 147, 11); } }

        public  static UIColor AcceptButtonHighlightedColor{ get { return UIColor.FromRGB (12, 98, 8); } }
        
        public  static UIColor LightButtonColor{ get { return UIColor.FromRGB (90, 90, 90); } }

        public  static UIColor LightButtonHighlightedColor{ get { return UIColor.FromRGB (50, 50, 50); } }

        public static UIColor CellBackgroundColor{ get { return UIColor.FromRGB (253, 253, 253); } }

        public static UIColor CellFirstLineTextColor{ get { return UIColor.FromRGB (120, 120, 120); } }

        public static UIColor CellSecondLineTextColor{ get { return UIColor.FromRGB (133, 133, 133); } }

        public static UIColor CellAddTextColor { get { return UIColor.FromRGB (75, 75, 75); } }

        public static void ApplyAppFont (this UIView instance)
        {
            TryToCast<UIButton> (instance, button => button.Font = ConvertFontToAppFont (button.Font));            
            TryToCast<UILabel> (instance, label => label.Font = ConvertFontToAppFont (label.Font));            
            TryToCast<UITextView> (instance, txt => txt.Font = ConvertFontToAppFont (txt.Font));                       
            TryToCast<UITextField> (instance, txt =>
            {
                txt.Font = ConvertFontToAppFont (txt.Font);
                var font = txt.ValueForKeyPath (new NSString ("_placeholderLabel.font")) as UIFont;
                if (font != null) {
                    txt.SetValueForKeyPath (ConvertFontToAppFont (font), new NSString ("_placeholderLabel.font"));
                }
            });

            TryToCast<GradientButton> (instance, btn => btn.TitleFont = ConvertFontToAppFont (btn.TitleFont));
            instance.Subviews.ForEach (sub => sub.ApplyAppFont ());            
        }

        public static void TryToCast<T> (object instance, Action<T> action)
        {
            if (instance is T) {
                action ((T)instance);
            }
        }

        public static UIFont ConvertFontToAppFont (UIFont font)
        {
            var f = new CTFont( font.Name, font.PointSize );
            var t = f.GetTraits ().SymbolicTraits;

            bool isBold = false;
            bool isItalic = false;

            if ( ( t.HasValue ) && ( (t.Value & CTFontSymbolicTraits.Bold ) == CTFontSymbolicTraits.Bold ) )
            {
                isBold = true;
            }


            if ( ( t.HasValue ) && ( (t.Value & CTFontSymbolicTraits.Italic) == CTFontSymbolicTraits.Italic ) )
            {
                isItalic = true;
            }
              
            f.Dispose ();

            if ( isBold )
            {
            return GetBoldFont (font.PointSize );
            }
            else if ( isItalic )
            {
                return GetItalicFont ( font.PointSize ) ;
            }
            else
            {
                return GetNormalFont ( font.PointSize );
            }
        }


    }

}

