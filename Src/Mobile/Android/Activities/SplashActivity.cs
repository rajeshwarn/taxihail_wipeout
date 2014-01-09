using Android.App;
using Android.OS;
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Cirrious.MvvmCross.Android.Platform;
using Cirrious.MvvmCross.Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true,
        Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    //[IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" }, DataScheme = "TaxiHailDemo")]
    public class SplashActivity : MvxBaseSplashScreenActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var setup = (Setup) MvxAndroidSetupSingleton.GetOrCreateSetup(ApplicationContext);

            if (Intent.Extras != null && Intent.Extras.ContainsKey("orderId"))
            {
                setup.SetParams(new Dictionary<string, string>
                {
                    {"orderId", Intent.Extras.GetString("orderId")}
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}