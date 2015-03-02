using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class DateTimeFormatConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = value as DateTime?;
            var format = (parameter as string) ?? "G";

            return date.HasValue
				? Format(date.Value.ToLocalTime(), format)
                : value;
        }


		private string Format(DateTime dateTime, string format)
        {
            return format.Equals("SDT")
				? dateTime.ToShortDateString() + " / " + dateTime.ToShortTimeString()
                : dateTime.ToString(format, CultureProvider.CultureInfo);
        }
    }
}