using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ToastHelper
    {
        public static ToastView Toast { get; set;}

        public static void Show(string message)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    Toast = new ToastView(message);

                    Toast.Show();
                });
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

