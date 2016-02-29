using System;
using System.Linq;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Views;
using System.Threading.Tasks;
using apcurium.MK.Common;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using MK.Common.Android.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ToastHelper
    {
        public static ToastView Toast { get; set;}

        private static bool IsTopMostViewSplashScreen()
        {
            var topMostWindow = UIApplication.SharedApplication.Windows.LastOrDefault();

			return topMostWindow.SelectOrDefault(window => window.RootViewController is SplashView || window.RootViewController is ExtendedSplashScreenView);
        }

		public static bool Show(string message)
		{
			try
			{
				//In the case where SharedApplication.Windows is null or empty, we return false.
			    if (UIApplication.SharedApplication.Windows == null || UIApplication.SharedApplication.Windows.None())
			    {
			        return false;
			    }

				//If we are displaying he splashscreen or extended splashscreen.
				if(IsTopMostViewSplashScreen())
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
			catch(Exception ex)
			{
                var logger = Mvx.Resolve<ILogger>();

			    logger.LogMessage("An error occurred while attempting to show toast, will retry in 5 seconds");
                logger.LogErrorWithCaller(ex);

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

