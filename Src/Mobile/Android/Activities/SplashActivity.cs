#if SOCIAL_NETWORKS
using SocialNetworks.Services;
using SocialNetworks.Services.MonoDroid;
using SocialNetworks.Services.OAuth;
#endif
using Android.App;
using Android.OS;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using Java.Lang;
using Cirrious.MvvmCross.Interfaces.Platform.Location;
using Cirrious.MvvmCross.Android.Platform;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices.Impl;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	//[IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" }, DataScheme = "TaxiHailDemo")]
	public class SplashActivity : MvxBaseSplashScreenActivity
    {
        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var setup = (Setup)MvxAndroidSetupSingleton.GetOrCreateSetup (ApplicationContext);

			if (this.Intent.Extras != null && this.Intent.Extras.ContainsKey ("orderId")) {
				setup.SetParams(new Dictionary<string, string> {
					{ "orderId", this.Intent.Extras.GetString("orderId") }
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
