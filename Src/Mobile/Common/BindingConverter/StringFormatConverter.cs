using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class StringFormatConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			if (value == null)
			{
				return null;
			}

			if (parameter == null)
			{
				return value;
			}

			var format = "{0:" + parameter.ToString()  + "}";

			return string.Format(culture, format, value);
        }
    }
}