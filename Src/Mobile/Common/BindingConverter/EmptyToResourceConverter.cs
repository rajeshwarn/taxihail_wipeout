using System;
using System.Globalization;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class EmptyToResourceConverter: MvxValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!string.IsNullOrEmpty(value as string)) {
				return value;
			}
			if(string.IsNullOrEmpty((string)parameter)) {
				return null;
			}
			return Mvx.Resolve<IAppResource>().GetString(parameter.ToString());
		}
	}
}

