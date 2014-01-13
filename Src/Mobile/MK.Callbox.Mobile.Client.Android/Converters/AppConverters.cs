using apcurium.MK.Booking.Mobile.Client.Converters;
using Cirrious.MvvmCross.Converters.Visibility;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.BindingConverter;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace apcurium.MK.Callbox.Mobile.Client.Converters
{


    public class AppConverters
    {
		public readonly MvxVisibilityValueConverter Visibility = new MvxVisibilityValueConverter();
		public readonly MvxInvertedVisibilityValueConverter InvertedVisibility = new MvxInvertedVisibilityValueConverter();
        public readonly BoolInverter BoolInverter = new BoolInverter();
		public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
		public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();
        public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter = new OrderStatusToTextColorConverter();
        public readonly ColorEnumToAndroidColorConverter ColorEnumToAndroidColor = new ColorEnumToAndroidColorConverter();

    }
}