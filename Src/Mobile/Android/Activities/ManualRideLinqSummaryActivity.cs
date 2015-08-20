using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/MainTheme",
		Label = "@string/ManualRideLinqSummaryActivityName",
        ScreenOrientation = ScreenOrientation.Portrait
      )]
    public class ManualRideLinqSummaryActivity : BaseBindingActivity<ManualRideLinqSummaryViewModel>
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            SetContentView(Resource.Layout.View_ManualRideLinqSummary);

            var lblSubTitle = FindViewById<TextView>(Resource.Id.lblSubTitle);
            lblSubTitle.Text = String.Format(this.Services().Localize["RideSummarySubTitleText"], this.Services().Settings.TaxiHail.ApplicationName);
        }

        public override void OnBackPressed()
        {
            ViewModel.GoToHome.ExecuteIfPossible();
        }
    }
}