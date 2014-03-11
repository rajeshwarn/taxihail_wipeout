using System;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class ViewServicesExtensions
	{
		public static ServicesExtensionPoint Services(this IMvxView activity)
		{
			return new ServicesExtensionPoint();
		}
	}
}

