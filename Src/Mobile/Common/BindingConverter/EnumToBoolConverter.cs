using System;
using System.Collections;
using System.Globalization;
using System.Linq;
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
			if (value == null || parameter == null)
			{
				return InvertResultIfNecessary (false);
			}

			var name = Enum.GetName(value.GetType(), value);

			if (name == null) 
			{
				return InvertResultIfNecessary (false);
			}

			var paramArray = parameter as IEnumerable;

			var valueIsInParam = paramArray == null
				? name.Equals(parameter.ToString())
				: paramArray.Cast<object>().Select(item => item.ToString()).Any(item => name.Equals(item));

			return InvertResultIfNecessary(valueIsInParam);
		}

		private bool InvertResultIfNecessary(bool value)
		{
			return _inverted 
				? !value
				: value;
		}
	}
}

