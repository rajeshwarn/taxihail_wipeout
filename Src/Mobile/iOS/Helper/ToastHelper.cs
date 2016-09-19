using System.Linq;
using System.Threading.Tasks;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.Client.Extensions;
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
			// We get the topmost window on the stack.
            var topMostWindow = UIApplication.SharedApplication.Windows.LastOrDefault();

			return topMostWindow.SelectOrDefault(window => window.RootViewController is SplashView || window.RootViewController is ExtendedSplashScreenView);
        }

		public static Task<bool> Show(string message)
		{
			return UIApplication.SharedApplication.InvokeOnMainThreadAsync(() =>
			{
				if (IsTopMostViewSplashScreen())
			    {
			        return false;
			    }

				//If we are displaying he splashscreen or extended splashscreen.
				if(IsTopMostViewSplashScreen())
				{
					return false;
				}

				Toast = new ToastView(message);

				Toast.Show();

				return true;
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

