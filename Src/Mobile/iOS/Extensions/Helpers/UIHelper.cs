using MonoTouch.UIKit;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public static class UIHelper
    {
        public static bool IsRetinaDisplay { get { return UIScreen.MainScreen.Scale > 1.0; } }

		public static float OnePixel
		{
			get{
				return IsRetinaDisplay ? .5f : 1f;
			}
		}

        public static bool Is4InchDisplay
        {
            get { return UIScreen.MainScreen.Bounds.Height > 500; }
        }

		public static bool IsOS7orHigher
		{
			get{
				var first = UIDevice.CurrentDevice.SystemVersion.Split('.')[0];
				return Convert.ToInt32(first) >= 7;
			}
		}

        public static float GetConvertedPixel(float pixel)
        {
            if (IsRetinaDisplay)
            {
                return pixel / 2f;
            }

            return pixel;
        }
    }
}

