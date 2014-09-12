using System;
using System.Xml.Serialization;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.Style
{
	public static class Theme
    {
		static ThemeValues _themeValues;
		static Theme()
		{
			using (var themeFile = typeof(Theme).Assembly.GetManifestResourceStream("Theme.xml"))
			{
				var serializer = new XmlSerializer(typeof(ThemeValues));
				_themeValues = (ThemeValues)serializer.Deserialize(themeFile);
			}
		}

        static UIColor _companyColor;
		public static UIColor CompanyColor
		{
            get{ return ColorHelper.ToUIColor(_themeValues.CompanyColor, ref _companyColor); }
		}

        static UIColor _loginColor;
        public static UIColor LoginColor
        {
            get{ return ColorHelper.ToUIColor(_themeValues.LoginColor, ref _loginColor); }
        }

		static UIColor _labelTextColor;
		public static UIColor LabelTextColor
		{
            get{ return ColorHelper.ToUIColor(_themeValues.LabelTextColor, ref _labelTextColor); }
		}

		static UIColor _buttonTextColor;
		public static UIColor ButtonTextColor
		{
            get{ return ColorHelper.ToUIColor(_themeValues.ButtonTextColor, ref _buttonTextColor); }
		}

        static UIColor _menuColor;
        public static UIColor MenuColor
        {
            get{ return ColorHelper.ToUIColor(_themeValues.MenuColor, ref _menuColor); }
        }

		public static bool IsLightContent
		{
			get
			{
                return ShouldHaveLightContent(CompanyColor);
			}
		}

        public static bool ShouldHaveLightContent(UIColor color)
        {
            var components = color.CGColor.Components;
            var darknessScore = (((components[0]*255) * 299) + ((components[1]*255) * 587) + ((components[2]*255) * 114)) / 1000;

            if (darknessScore >= 125) {
                return false;
            }
            return true;
        }

        public static UIColor GetTextColor(UIColor loginColor)
        {
            return ShouldHaveLightContent(loginColor) ? UIColor.White : UIColor.Black;
        }
    }

	public class ThemeValues 
	{
        public string CompanyColor { get; set; }
        public string LoginColor { get; set; }
		public string LabelTextColor { get; set; }
		public string ButtonTextColor { get; set; }
        public string MenuColor { get; set; }
	}
}

