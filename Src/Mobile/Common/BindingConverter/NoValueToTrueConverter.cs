using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.Localization;
using System.Collections;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class NoValueToTrueConverter : MvxBaseValueConverter, IMvxServiceConsumer<IMvxTextProvider>
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
