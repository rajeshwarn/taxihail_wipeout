
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppStyle
    {
        public AppStyle()
        {
        }
        public enum ButtonColor
        {
            Black,
            Grey,
            Green,
            Red,
            Gold,
            Blue,
            DarkBlue,
			Silver
		}
        ;
        private static Dictionary<ButtonColor, UIColor[]> _buttonColors = new Dictionary<ButtonColor, UIColor[]>(){ 
            { ButtonColor.Black, new UIColor[] { UIColor.FromRGB(39,40,40), UIColor.FromRGB(36,37,37), UIColor.Black, UIColor.FromRGB(50,50,50) } },
            { ButtonColor.Grey, new UIColor[] { UIColor.FromRGB(240,240,240), UIColor.FromRGB(222,222,222), UIColor.FromRGB(200,200,200) } },
            { ButtonColor.Green, new UIColor[] { UIColor.FromRGB(143,207,15), UIColor.FromRGB(87,177,9) } },
            { ButtonColor.Red, new UIColor[] { UIColor.FromRGB(248,76,76), UIColor.FromRGB(244,46,46) } },
            { ButtonColor.Gold, new UIColor[] { UIColor.FromRGB(199,158,16), UIColor.FromRGB(206,164,18), UIColor.FromRGB(215,177,10), UIColor.FromRGB(244,206,40) } },
            { ButtonColor.Blue, new UIColor[] { UIColor.FromRGB(15,94,163) } },
            { ButtonColor.DarkBlue, new UIColor[] { UIColor.FromRGB(13,69,119) } },
			{ ButtonColor.Silver, new UIColor[] { UIColor.FromRGB(210,208,207), UIColor.FromRGB(248,247,244) } }
        };
		private static Dictionary<ButtonColor, UIColor> _textShadowColor = new Dictionary<ButtonColor, UIColor>(){ 
			{ ButtonColor.Black, UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
			{ ButtonColor.Grey, UIColor.FromRGBA(255, 255, 255, 1f) },
            { ButtonColor.Green, UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
            { ButtonColor.Red,  UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
            { ButtonColor.Gold, UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
            { ButtonColor.Blue, UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
            { ButtonColor.DarkBlue, UIColor.FromRGBA(0f, 0f, 0f, 0.5f) },
			{ ButtonColor.Silver,  UIColor.FromRGBA(255, 255, 255, 0.5f) }
        };
		private static Dictionary<ButtonColor, float[]> _buttonColorLocations = new Dictionary<ButtonColor, float[]>(){ 
            { ButtonColor.Black, new float[] { 0f, 0.5f, 0.53f, 1f } },
            { ButtonColor.Grey, new float[] { 0f, 0.93f, 1f } },
            { ButtonColor.Green, new float[] { 0f, 1f } },
            { ButtonColor.Red, new float[] { 0f, 1f } },
            { ButtonColor.Gold, new float[] { 0f, 0.5f, 0.5f, 1f } },
            { ButtonColor.Blue, new float[] { 1f } },
            { ButtonColor.DarkBlue, new float[] { 1f } },
			{ ButtonColor.Silver, new float[] { 1f, 0f } }
        };
        private static Dictionary<ButtonColor, UIColor> _buttonStrokeColors = new Dictionary<ButtonColor, UIColor>(){ 
            { ButtonColor.Black, UIColor.Black },
            { ButtonColor.Grey, UIColor.FromRGB(155,155,155) },
            { ButtonColor.Green, UIColor.FromRGB(69,103,24) },
            { ButtonColor.Red, UIColor.FromRGB(182,53,64) },
            { ButtonColor.Gold, UIColor.FromRGB(90,74,6) },
            { ButtonColor.Blue, UIColor.FromRGB(7,34,57) },
            { ButtonColor.DarkBlue, UIColor.FromRGB(7,34,57) }
        };
        private static Dictionary<ButtonColor, ShadowSetting> _buttonInnerShadows = new Dictionary<ButtonColor, ShadowSetting>(){ 
            { ButtonColor.Green, new ShadowSetting() { BlurRadius = 1, Offset = new SizeF( 0, 1 ), Color = UIColor.FromRGBA( 255,255,255, 0.5f ) } },
            { ButtonColor.Red, new ShadowSetting() { BlurRadius = 1, Offset = new SizeF( 0, 1 ), Color = UIColor.FromRGBA( 255,255,255, 0.5f ) } },
			{ ButtonColor.Silver, new ShadowSetting() { BlurRadius = 1, Offset = new SizeF( 0, -1 ), Color = UIColor.FromRGB( 80,79,78 ) } }
        };
        private static Dictionary<ButtonColor, ShadowSetting> _buttonDropShadows = new Dictionary<ButtonColor, ShadowSetting>(){ 
            { ButtonColor.Blue, new ShadowSetting() { BlurRadius = 1, Offset = new SizeF( 0, 1 ), Color = UIColor.FromRGB( 38,107,167 ) } },
            { ButtonColor.DarkBlue, new ShadowSetting() { BlurRadius = 1, Offset = new SizeF( 0, 1 ), Color = UIColor.FromRGB( 38,105,164 ) } }
        };

        public static float ButtonStrokeLineWidth { get { return 2f; } }

        public static float ButtonCornerRadius { get { return 2f; } }

        public static UIColor[] GetButtonColors(ButtonColor color)
        {
            return _buttonColors [color];
        }

        public static float[] GetButtonColorLocations(ButtonColor color)
        {
            return _buttonColorLocations [color];
        }

        public static UIColor GetButtonTextShadowColor(ButtonColor color)
        {
            return _textShadowColor [color];
        }

        public static ShadowSetting GetInnerShadow(ButtonColor color)
        {
            if (_buttonInnerShadows.ContainsKey(color))
            {
                return _buttonInnerShadows [color];
            } else
            {
                return null;
            }
        }

        public static ShadowSetting GetDropShadow(ButtonColor color)
        {
            if (_buttonDropShadows.ContainsKey(color))
            {
                return _buttonDropShadows [color];
            } else
            {
                return null;
            }
        }

        public static UIColor GetButtonStrokeColor(ButtonColor color)
        {
            return _buttonStrokeColors [color];
        }

        public static UIColor GreyText { get { return UIColor.FromRGB(101, 101, 101); } }

        public static UIColor LightBlue { get { return UIColor.FromRGB(188, 217, 242); } }

        public static UIColor DarkText { get { return UIColor.FromRGB(57, 44, 11); } }

		public static UIColor TitleTextColor { get { return UIColor.FromRGB(66, 63, 58); } }

		public static UIFont NormalTextFont { get { return UIFont.FromName( "HelveticaNeue", CellFontSize ); } }

		public static UIFont BoldTextFont { get { return UIFont.FromName( "HelveticaNeue-Bold", CellFontSize ); } }

        public static string ButtonFontName { get { return "HelveticaNeue-Bold"; } }

		public static UIFont CellFont { get { return UIFont.FromName( "HelveticaNeue-Bold", CellFontSize ); } }

		public static UIFont CellSmallFont { get { return UIFont.FromName( "HelveticaNeue-Bold", CellSmallFontSize ); } }

		public static float CellFontSize { get { return 14f; } }

		public static float CellSmallFontSize { get { return 10f; } }

        public static float ButtonFontSize { get { return 14f; } }

        public static UIFont GetButtonFont(float size)
        {
            return UIFont.FromName(ButtonFontName, size);
        }
        
        public  static  UIColor AcceptButtonColor{ get { return UIColor.FromRGB(19, 147, 11); } }

        public  static UIColor AcceptButtonHighlightedColor{ get { return UIColor.FromRGB(12, 98, 8); } }
        
        public  static UIColor CancelButtonColor{ get { return UIColor.FromRGB(215, 0, 2); } }

        public  static UIColor CancelButtonHighlightedColor{ get { return UIColor.FromRGB(136, 0, 2); } }
        
        public  static UIColor LightButtonColor{ get { return UIColor.FromRGB(90, 90, 90); } }

        public  static UIColor LightButtonHighlightedColor{ get { return UIColor.FromRGB(50, 50, 50); } }

		public static UIColor CellBackgroundColor{ get{ return UIColor.FromRGB(253, 253, 253); } }
		public static UIColor CellFirstLineTextColor{ get{ return UIColor.FromRGB(77, 77, 77); } }
		public static UIColor CellSecondLineTextColor{ get{ return UIColor.FromRGB(133, 133, 133); } }
//        public static UIColor NavBarColor
//        {
//             private void LoadBackgroundNavBar()
//        {
//            NavigationBar.TintColor = UIColor.FromRGB(0, 78, 145);
//
//            //It might crash on iOS version smaller than 5.0
//            try 
//            {
//                NavigationBar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
//            }
//            catch
//            {
//            }
//        }    
//            get { return UIColor.FromRGB(174, 181, 137); }
//        }
    }

    public class ShadowSetting
    {
        public float BlurRadius { get; set; }

        public SizeF Offset { get; set; }

        public UIColor Color { get; set; }

    }
}

