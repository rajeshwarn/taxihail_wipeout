using System;
using Android.App;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Activities;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class ToastHelper
    {
        public static CustomToast Toast { get; set;}

        public static bool Show(Activity owner, string message)
        {
            if (owner is SplashActivity)
            {
                return false;
            }

            Toast = new CustomToast(owner, message);

            return Toast.Show();
        }

        public static void Dismiss()
        {
            if(Toast != null)
            {
                Toast.Dismiss();
            }
        }
    }
}

