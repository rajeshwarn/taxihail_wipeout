using System;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class ColorExtensions
	{
        public static float[] ToArray(this UIColor instance)
        {
			float r=0, g=0, b=0, a=1;
			instance.GetRGBA( out r, out g, out b, out a );

			return new float[] { r, g, b, a };
        }

	}
}