using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : MvxSplashScreenActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			//TODO: [MvvmCross v3] This code has no effect, params are ignored in Setup
			/*
			var setup = (Setup) MvxAndroidSetupSingleton.GetOrCreateSetup(ApplicationContext);
			if (Intent.Extras != null && Intent.Extras.ContainsKey("orderId"))
			{
				setup.SetParams(new Dictionary<string, string>
					{
						{"orderId", Intent.Extras.GetString("orderId")}
					});
            }*/
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}