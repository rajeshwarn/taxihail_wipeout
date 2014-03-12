using System;
using System.Collections;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class NoValueToTrueConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return true;               
            }
            var enumerable = value as IEnumerable;
            if(enumerable != null)
            {
                return !enumerable.GetEnumerator().MoveNext();
            }
            return false;
                
        }

    }
}
