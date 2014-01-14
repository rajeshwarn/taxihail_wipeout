using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : MvxSplashScreenActivity
    {
        Dictionary<string, string> _params = new Dictionary<string, string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			if (Intent.Extras != null && Intent.Extras.ContainsKey("orderId"))
			{
                this._params = new Dictionary<string, string>
					{
						{"orderId", Intent.Extras.GetString("orderId")}
					};
            }
        }

        protected override void TriggerFirstNavigate()
        {
            // Overriden in order to pass params
            var starter = Mvx.Resolve<IMvxAppStart>();
            starter.Start(_params);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}