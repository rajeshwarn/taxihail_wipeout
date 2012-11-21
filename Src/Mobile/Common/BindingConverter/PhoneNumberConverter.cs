using System;
using Cirrious.MvvmCross.Converters;
using System.Linq;
using System.Text.RegularExpressions;

namespace apcurium.MK.Booking.Mobile
{
    public class PhoneNumberConverter: MvxBaseValueConverter
    {
        public override object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var candidate = (string)value;
            if(string.IsNullOrWhiteSpace(candidate)) return value;

            try
            {
                var cleaned = new string(candidate.Where(c => Char.IsDigit(c)).ToArray());
                return Regex.Replace(cleaned, @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
            }
            catch
            {
                return value;
            }
        }
    }
}

