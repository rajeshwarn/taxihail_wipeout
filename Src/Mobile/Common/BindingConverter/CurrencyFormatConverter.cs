using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class CurrencyFormatConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			if (value == null)
			{
				return value;
			}

            var currency = System.Convert.ToDouble(value);

            return CultureProvider.FormatCurrency(currency);
        }
    }
}