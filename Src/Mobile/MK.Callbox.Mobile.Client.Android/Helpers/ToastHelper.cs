using System;
using Android.App;
using apcurium.MK.Callbox.Mobile.Client.Activities;
using apcurium.MK.Callbox.Mobile.Client.Controls.Message;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class ToastHelper
    {
        public static CustomToast Toast { get; set;}

        public static bool Show(Activity owner, string message)
        {
            if (owner is SplashScreenActivity)
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

