using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
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
        Label = "ManualRideLinqSummaryActivity",
        ScreenOrientation = ScreenOrientation.Portrait
      )]  
    public class ManualRideLinqSummaryActivity : BaseBindingActivity<ManualRideLinqSummaryViewModel>
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            SetContentView(Resource.Layout.View_ManualRideLinqSummary);
        }

        public override void OnBackPressed()
        {
            ViewModel.GoToHome.ExecuteIfPossible();
        }
    }
}