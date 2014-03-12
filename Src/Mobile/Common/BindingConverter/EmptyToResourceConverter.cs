using System;
using System.Globalization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class EmptyToResourceConverter: MvxValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!string.IsNullOrEmpty(value as string)) {
				return value;
			}

			return string.IsNullOrEmpty((string)parameter) 
                ? null 
                : Mvx.Resolve<ILocalization>()[parameter.ToString()];
		}
	}
}

