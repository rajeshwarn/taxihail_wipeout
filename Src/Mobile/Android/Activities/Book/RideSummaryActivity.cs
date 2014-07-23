using System;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Book", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait,
        ClearTaskOnLaunch = true, FinishOnTaskLaunch = true)]
    public class RideSummaryActivity : BaseBindingActivity<RideSummaryViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Book_RideSummaryPage);
            var lblSubTitle = FindViewById<TextView>(Resource.Id.lblSubTitle);
			lblSubTitle.Text = String.Format(this.Services().Localize["RideSummarySubTitleText"], this.Services().Settings.ApplicationName);

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
                ViewModel.ProcessOrderRating();

                if (!ViewModel.CanUserLeaveScreen ())
                {
                    return false;
                }
			}

			return base.OnKeyDown(keyCode, e);
		}
    }
}