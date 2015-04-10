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
using Android.Views.InputMethods;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/MainTheme",
        Label = "ManualRideLinqStatusActivity",
        ScreenOrientation = ScreenOrientation.Portrait
      )]  
    public class ManualRideLinqStatusActivity : BaseBindingActivity<ManualRideLinqStatusViewModel>
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            SetContentView(Resource.Layout.View_ManualRideLinqStatus);

            var im = (InputMethodManager)GetSystemService(InputMethodService);
            if (im != null)
            {
                im.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.None);
            }
        }

    }
}