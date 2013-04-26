using System;
using Android.Views;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class DisplayExtensions
	{
		
		const int referenceWidth = 640;
		const int referenceHeight = 1136;

		public static float GetHorizontalScale(this Display screenSize)
		{
			return (float)screenSize.Width/(float)referenceWidth;
		}
		public static float GetVerticalScale(this Display screenSize)
		{
			return (float)screenSize.Height/(float)referenceHeight;
		}

		
		public static PointF GetScale(this Display screenSize)
		{
			return new PointF(screenSize.GetHorizontalScale(),screenSize.GetVerticalScale());
		}
	}
}

