using Android.App;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class ViewServicesExtensions
	{
		public static ServicesExtensionPoint Services(this Activity activity)
		{
			return new ServicesExtensionPoint();
		}
			
		public static ServicesExtensionPoint Services(this FrameLayout view)
		{
			return new ServicesExtensionPoint();
		}
	}
}

