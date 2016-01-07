using System;
using Android.App;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;

namespace TaxiHail.Shared.Helpers
{
    public static class ToastHelper
    {
        public static CustomToast Toast { get; set;}

        public static void Show(Activity owner, string message)
        {
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

