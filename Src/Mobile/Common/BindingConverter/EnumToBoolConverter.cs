using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class EnumToBoolConverter: MvxValueConverter
	{
		private bool _inverted;

		public EnumToBoolConverter(bool inverted = false)
		{
			_inverted = inverted;
		}

		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return InvertResultIfNecessary (false);
			}

			var name = Enum.GetName(value.GetType(), value);

			if (name == null) 
			{
				return InvertResultIfNecessary (false);
			}
			return InvertResultIfNecessary(name.Equals(parameter.ToString()));
		}

		private bool InvertResultIfNecessary(bool value)
		{
			return _inverted 
				? !value
				: value;
		}
	}
}

