using System;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ColorHelper
    {
        public static UIColor ToUIColor(string hexaDecimaleValue, ref UIColor color)
        {
            if (color == null)
            {
                var red = Convert.ToInt32(hexaDecimaleValue.Substring(1, 2), 16) / 255f;
                var green = Convert.ToInt32(hexaDecimaleValue.Substring(3, 2), 16) / 255f;
                var blue = Convert.ToInt32(hexaDecimaleValue.Substring(5, 2), 16) / 255f;

                var alpha = 1f;
                if (hexaDecimaleValue.Length > 7)
                {
                    alpha = Convert.ToInt32(hexaDecimaleValue.Substring(7, 2), 16) / 255f;
                }
                color = UIColor.FromRGBA(red, green, blue, alpha);
            }
            return color;
        }
    }
}

