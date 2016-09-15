using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.App;
using apcurium.MK.Callbox.Mobile.Client.Activities;
using apcurium.MK.Callbox.Mobile.Client.Controls.Message;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class ToastHelper
    {
        public static CustomToast Toast { get; set; }

        public static async Task<bool> Show(Activity owner, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (owner is SplashScreenActivity)
            {
                return false;
            }

            var dispatcher = Mvx.Resolve<IMvxMainThreadDispatcher>();

            dispatcher.RequestMainThreadAction(() =>
            {
                try
                {
                    Toast = new CustomToast(owner, message);
                    var isShown = Toast.Show();

                    tcs.TrySetResult(isShown);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);

                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            });
            
            return await tcs.Task.ConfigureAwait(false);
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

