using System;
using System.Globalization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.Interfaces.Localization;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class CoordinateConverter : MvxBaseValueConverter, IMvxServiceConsumer<IMvxTextProvider>
    {
        public override object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return new CoordinateViewModel();
        }

        public override object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }
}