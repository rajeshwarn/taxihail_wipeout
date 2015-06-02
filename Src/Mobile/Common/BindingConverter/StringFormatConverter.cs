using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class StringFormatConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			if (parameter == null)
			{
				return value;
			}

			return string.Format(culture, parameter.ToString(), value);
        }
    }
}