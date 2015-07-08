using System;
using Cirrious.CrossCore.Converters;
using System.Globalization;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class DialCodeConverter:MvxValueConverter
	{
		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            CountryISOCode countryISOCode = value as CountryISOCode;

            if (countryISOCode != null)
			{
                return "+" + CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryISOCode)).CountryDialCode;
			}

			return null;
		}
	}
}