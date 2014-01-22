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

		static UIColor _backgroundColor;
		public static UIColor BackgroundColor
		{
			get{ return ToUIColor(_themeValues.LoginBackgroundColor, ref _backgroundColor); }
		}

		static UIColor _mainButtonBackgroundColor;
		public static UIColor MainButtonBackgroundColor
		{
			get{ return ToUIColor(_themeValues.MainButtonBackgroundColor, ref _mainButtonBackgroundColor); }
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

		static UIColor ToUIColor(string hexaDecimaleValue, ref UIColor color)
		{
			if (color == null)
			{
				var red = Convert.ToInt32(hexaDecimaleValue.Substring(1, 3), 16) / 255f;
				var green = Convert.ToInt32(hexaDecimaleValue.Substring(3, 2), 16) / 255f;
				var blue = Convert.ToInt32(hexaDecimaleValue.Substring(5, 2), 16) / 255f;
				color =  UIColor.FromRGB(red, green, blue);
			}
			return color;
		}
    }

	public class ThemeValues 
	{
		public string LoginBackgroundColor { get; set; }
		public string MainButtonBackgroundColor { get; set; }
		public string LabelTextColor { get; set; }
		public string ButtonTextColor { get; set; }
	}


}

