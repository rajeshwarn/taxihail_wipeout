using System;
using System.Globalization;
using Cirrious.MvvmCross.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class EnumToBoolConverter: MvxBaseValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null) return false;
			var name = Enum.GetName(value.GetType(), value);
			if(name == null) return false;
			return name.Equals(parameter);
		}
	}
}

