using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public static class UIHelper
    {
        public static bool IsRetinaDisplay { get { return UIScreen.MainScreen.Scale > 1.0; } }

        public static bool Is4InchDisplay
        {
            get { return UIScreen.MainScreen.Bounds.Height > 500; }
        }

    }
}

