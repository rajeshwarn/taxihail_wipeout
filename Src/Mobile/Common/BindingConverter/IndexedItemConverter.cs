using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using System.Collections;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class IndexedItemConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			var index = int.Parse (parameter.ToString ());
			return (value as Array).GetValue (index);
        }
    }
}
