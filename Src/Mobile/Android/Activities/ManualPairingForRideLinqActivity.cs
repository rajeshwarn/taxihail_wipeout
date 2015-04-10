using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Text.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/MainTheme",
        Label = "ManualPairingForRideLinqActivity",
        ScreenOrientation = ScreenOrientation.Portrait
      )]  
    public class ManualPairingForRideLinqActivity : BaseBindingActivity<ManualPairingForRideLinqViewModel>
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            SetContentView(Resource.Layout.View_ManualPairingForRideLinq);

            var pairingCodeTextField1 = FindViewById<EditText>(Resource.Id.PairingCode1);
            var pairingCodeTextField2 = FindViewById<EditText>(Resource.Id.PairingCode2);

            PairingCodeBehavior.ApplyTo(pairingCodeTextField1, pairingCodeTextField2);

            var im = (InputMethodManager)GetSystemService(InputMethodService);
            if (im != null)
            {
                im.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.None);
            }
        }

    }
}