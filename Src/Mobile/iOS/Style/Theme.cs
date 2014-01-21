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
			get
			{ 
				if (_backgroundColor == null)
				{
					_backgroundColor = ToUIColor(_themeValues.LoginBackgroundColor);
				}
				return _backgroundColor; 
			}
		}

		static UIColor _mainButtonBackgroundColor;
		public static UIColor MainButtonBackgroundColor
		{
			get
			{ 
				if (_mainButtonBackgroundColor == null)
				{
					_mainButtonBackgroundColor = ToUIColor(_themeValues.MainButtonBackgroundColor);
				}
				return _mainButtonBackgroundColor; 
			}
		}

		static UIColor ToUIColor(string hexaDecimaleValue)
		{
			var red = Convert.ToInt32(hexaDecimaleValue.Substring(0, 2), 16) / 255f;
			var green = Convert.ToInt32(hexaDecimaleValue.Substring(2, 2), 16) / 255f;
			var blue = Convert.ToInt32(hexaDecimaleValue.Substring(4, 2), 16) / 255f;
			return UIColor.FromRGB(red, green, blue);
		}
    }

	public class ThemeValues 
	{
		public string LoginBackgroundColor { get; set; }
		public string MainButtonBackgroundColor { get; set; }
	}


}

