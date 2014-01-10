using System;
using System.Globalization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class EmptyToResourceConverter: MvxBaseValueConverter, IMvxServiceConsumer<ILocalization>
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!string.IsNullOrEmpty(value as string)) {
				return value;
			}

			return string.IsNullOrEmpty((string)parameter) 
                ? null 
                : this.GetService()[parameter.ToString()];
		}
	}
}

