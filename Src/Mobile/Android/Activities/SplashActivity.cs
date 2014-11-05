using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : MvxSplashScreenActivity
    {
        Dictionary<string, string> _params = new Dictionary<string, string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			DensityHelper.OutputToConsole();

            if (Intent.Extras != null)
            {
                _params = new Dictionary<string, string>();

                if (Intent.Extras.ContainsKey("orderId"))
                {
                    _params.Add("orderId", Intent.Extras.GetString("orderId"));
                }
                if (Intent.Extras.ContainsKey("isPairingNotification"))
                {
                    _params.Add("isPairingNotification", Intent.Extras.GetString("isPairingNotification"));
                }
            }
        }

        protected override void TriggerFirstNavigate()
        {
            var paymentService = TinyIoCContainer.Current.Resolve<IPaymentService>();
            paymentService.ClearPaymentSettingsFromCache();

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