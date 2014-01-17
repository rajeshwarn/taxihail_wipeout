using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using ServiceStack.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class OrderStatusToImageNameConverter: MvxValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;

			var name = Enum.GetName(value.GetType(), value);
			if (name == null) {
				throw new ArgumentException ("Unknown Enum member - please update this converter");
			}
			name = name.ToLower ();
			return string.Format ("orderstatus_{0}", name);
		}
	}
}

