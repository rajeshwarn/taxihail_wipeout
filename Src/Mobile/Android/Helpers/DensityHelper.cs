using System;
using Android.Util;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class DensityHelper
	{
		public static void OutputToConsole()
		{
			var density = "";
			switch (Application.Context.Resources.DisplayMetrics.DensityDpi) 
			{
				case Android.Util.DisplayMetricsDensity.Low:
					density = "LDPI";
					break;
				case Android.Util.DisplayMetricsDensity.Medium:
					density = "MDPI";
					break;
				case Android.Util.DisplayMetricsDensity.High:
					density = "HDPI";
					break;
				case Android.Util.DisplayMetricsDensity.Xhigh:
					density = "XHDPI";
					break;
			}

			Console.WriteLine (string.Format ("Device is {0}", density));
		}
	}
}

