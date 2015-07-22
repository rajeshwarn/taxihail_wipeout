using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;

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

            var lblSubTitle = FindViewById<TextView>(Resource.Id.lblSubTitle);
            lblSubTitle.Text = String.Format(this.Services().Localize["RideSummarySubTitleText"], this.Services().Settings.TaxiHail.ApplicationName);

            var listView = FindViewById<MvxListView> (Resource.Id.RatingListView);
            listView.Divider = null;
            listView.DividerHeight = 0;

            ViewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "RatingList" && ViewModel.RatingList != null)
                    {
                        // Dynamically change height of list
                        var item = LayoutInflater.Inflate(listView.Adapter.ItemTemplateId, null);
                        item.Measure(Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
                            Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                        listView.LayoutParameters = new LinearLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, item.MeasuredHeight * ViewModel.RatingList.Count);
                    }
                };
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.RateOrderAndNavigateToHome.ExecuteIfPossible();

                return false;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}