using System;
using Android.App;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Activities;
using Cirrious.CrossCore;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class ToastHelper
    {
        public static CustomToast Toast { get; set;}

        public static void Show(Activity owner, string message)
        {
            if (owner is SplashActivity)
            {
                Mvx.Resolve<IConnectivityService>().ToastDismissed();
                return;
            }

            Toast = new CustomToast(owner, message);

            Toast.Show();
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

