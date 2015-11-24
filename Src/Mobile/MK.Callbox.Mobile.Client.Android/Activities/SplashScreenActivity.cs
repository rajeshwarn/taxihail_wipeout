using System;
using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class SplashScreenActivity : MvxSplashScreenActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
        }

		protected override void TriggerFirstNavigate()
		{
			Mvx.RegisterSingleton<ILogger>(new LoggerImpl());
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}