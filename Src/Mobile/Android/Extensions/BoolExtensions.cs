using System;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class BoolExtentions
    {
        public static ViewStates ToVisibility(this bool thisBool, bool invert = false)
        {
            if(invert)
            {
                return thisBool ? ViewStates.Gone : ViewStates.Visible;
            }

            return thisBool ? ViewStates.Visible : ViewStates.Gone;
        }
    }
}

