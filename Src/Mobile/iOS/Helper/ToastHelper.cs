using System.Threading.Tasks;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.Client.Extensions;


namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class ToastHelper
    {
        public static ToastView Toast { get; set;}

        public static Task<bool> Show(string message)
        {
			return UIApplication.SharedApplication.InvokeOnMainThreadAsync(() =>
			{
				if (IsWindowsSplashScreen())
				{
					return false;
				}

				Toast = new ToastView(message);

				Toast.Show();

				return true;
			});
        }

		private static bool IsWindowsSplashScreen()
		{
			return UIApplication.SharedApplication.Windows != null
				&& UIApplication.SharedApplication.Windows.Length > 0
				&& UIApplication.SharedApplication.Windows[UIApplication.SharedApplication.Windows.Length - 1].RootViewController != null
				&& UIApplication.SharedApplication.Windows[UIApplication.SharedApplication.Windows.Length - 1].RootViewController is SplashView;
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

