using UIKit;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public static class UIHelper
    {
        public static bool IsRetinaDisplay { get { return UIScreen.MainScreen.Scale > 1.0; } }

		public static nfloat OnePixel
		{
			get
            {
                return 1f / UIScreen.MainScreen.Scale;
			}
		}

        public static bool Is35InchDisplay
        {
            get { return UIScreen.MainScreen.Bounds.Height < 500; }
        }

        public static bool IsOS7
        {
            get{
                return Convert.ToInt32(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]) == 7;
            }
        }

        public static bool IsOS8orHigher
        {
            get{
                return Convert.ToInt32(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]) >= 8;
            }
        }

        public static nfloat GetConvertedPixel(nfloat pixel)
        {
            return pixel / UIScreen.MainScreen.Scale;
        }
    }
}

