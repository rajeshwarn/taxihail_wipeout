using apcurium.MK.Booking.Mobile.Client.Converters;
using Cirrious.MvvmCross.Converters.Visibility;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.BindingConverter;

namespace apcurium.MK.Callbox.Mobile.Client.Converters
{


    public class AppConverters
    {
        public readonly CoordinateConverter CoordinateConverter = new CoordinateConverter();
        public readonly MvxVisibilityConverter Visibility = new MvxVisibilityConverter();
        public readonly MvxInvertedVisibilityConverter InvertedVisibility = new MvxInvertedVisibilityConverter();
        public readonly BoolInverter BoolInverter = new BoolInverter();
		public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
		public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();
        public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter = new OrderStatusToTextColorConverter();
        public readonly ColorEnumToAndroidColorConverter ColorEnumToAndroidColor = new ColorEnumToAndroidColorConverter();

    }
}