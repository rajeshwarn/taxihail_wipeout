using Android.App;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Extensions;
using Android.Views;
using CrossUI.Core.Elements.Dialog;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class ViewServicesExtensions
	{
		public static ServicesExtensionPoint Services(this Activity activity)
		{
			return new ServicesExtensionPoint();
		}
			
		public static ServicesExtensionPoint Services(this View view)
		{
			return new ServicesExtensionPoint();
		}

		public static ServicesExtensionPoint Services(this IElement view)
		{
			return new ServicesExtensionPoint();
		}
	}
}

