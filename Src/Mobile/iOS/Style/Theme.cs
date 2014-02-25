using System;
using System.Xml.Serialization;
using MonoTouch.UIKit;

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
            get{ return ToUIColor(_themeValues.CompanyColor, ref _companyColor); }
		}

		static UIColor _labelTextColor;
		public static UIColor LabelTextColor
		{
			get{ return ToUIColor(_themeValues.LabelTextColor, ref _labelTextColor); }
		}

		static UIColor _buttonTextColor;
		public static UIColor ButtonTextColor
		{
			get{ return ToUIColor(_themeValues.ButtonTextColor, ref _buttonTextColor); }
		}

		public static bool IsLightContent
		{
			get
			{
				var components = CompanyColor.CGColor.Components;
				var darknessScore = (((components[0]*255) * 299) + ((components[1]*255) * 587) + ((components[2]*255) * 114)) / 1000;

				if (darknessScore >= 125) {
					return false;
				}
				return true;
			}
		}

		private static UIColor ToUIColor(string hexaDecimaleValue, ref UIColor color)
		{
			if (color == null)
			{
                var red = Convert.ToInt32(hexaDecimaleValue.Substring(1, 2), 16) / 255f;
				var green = Convert.ToInt32(hexaDecimaleValue.Substring(3, 2), 16) / 255f;
				var blue = Convert.ToInt32(hexaDecimaleValue.Substring(5, 2), 16) / 255f;
				color =  UIColor.FromRGB(red, green, blue);
			}
			return color;
		}
    }

	public class ThemeValues 
	{
        public string CompanyColor { get; set; }
		public string LabelTextColor { get; set; }
		public string ButtonTextColor { get; set; }
	}


}

