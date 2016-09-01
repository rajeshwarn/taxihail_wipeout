using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Views;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ToastHelper
    {
        public static ToastView Toast { get; set;}

        public static bool Show(string message)
        {
            if(UIApplication.SharedApplication.Windows != null 
                && UIApplication.SharedApplication.Windows.Length > 0 
                && UIApplication.SharedApplication.Windows[UIApplication.SharedApplication.Windows.Length-1].RootViewController != null
                && UIApplication.SharedApplication.Windows[UIApplication.SharedApplication.Windows.Length-1].RootViewController is SplashView)
            {
                return false;
            }

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    Toast = new ToastView(message);

                    Toast.Show();
                });

            return true;
        }

        public static void Dismiss()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    if(Toast != null)
                    {
                        Toast.Dismiss();
                    }
                });
        }

        public static void DismissNoAnimation()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    if(Toast != null)
                    {
                        Toast.DismissNoAnimation();
                    }
                });
        }
    }
}

