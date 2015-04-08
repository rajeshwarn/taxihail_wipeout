using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
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
        }

    }
}