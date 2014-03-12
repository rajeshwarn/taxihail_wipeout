using System;
using apcurium.MK.Booking.Mobile.Extensions;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class ViewServicesExtensions
	{
		public static ServicesExtensionPoint Services(this UIView view)
		{
			return new ServicesExtensionPoint();
		}

		public static ServicesExtensionPoint Services(this IMvxView viewController)
		{
			return new ServicesExtensionPoint();
		}
	}
}

