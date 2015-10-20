using System;
using Android.App;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class SplashScreenActivity : MvxSplashScreenActivity
    {
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}