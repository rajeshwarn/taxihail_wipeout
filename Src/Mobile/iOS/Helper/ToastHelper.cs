using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Views;
using System.Threading.Tasks;
using apcurium.MK.Common;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ToastHelper
    {
        public static ToastView Toast { get; set;}

		public static bool Show(string message)
		{
			try
			{
				UIApplication.EnsureUIThread();

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
			catch
			{
				// Attempted to show Toast before the app is are ready.
				Task.Run(async () =>
					{
						// retrying asynchronously in 5 seconds to make sure the app is loaded
						await Task.Delay(TimeSpan.FromSeconds(5));
						var service = Mvx.Resolve<IConnectivityService>();
						UIApplication.SharedApplication.InvokeOnMainThread(() => service.HandleToastInNewView());
					}).FireAndForget();

				return false;
			}
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

