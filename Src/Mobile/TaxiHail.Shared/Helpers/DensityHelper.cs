using System;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class DensityHelper
	{
		public static void OutputToConsole()
		{
            Console.WriteLine("Device is " + Application.Context.Resources.DisplayMetrics.DensityDpi);
		}
	}
}

