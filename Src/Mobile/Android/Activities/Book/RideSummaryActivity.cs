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
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "@string/BookSummaryActivityName", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait,
        ClearTaskOnLaunch = true, FinishOnTaskLaunch = true)]
    public class RideSummaryActivity : BaseBindingActivity<RideSummaryViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Book_RideSummaryPage);
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

		    var t = new System.Timers.Timer() { Interval = 3000 };
		    t.Elapsed += (s, e) =>
		    {
		        Console.WriteLine("Can rate : {0}", ViewModel.CanRate.ToString());
                Console.WriteLine("Can leave screen: {0}", ViewModel.CanUserLeaveScreen().ToString());
                Console.WriteLine("Need gratuity: {0}", ViewModel.NeedToSelectGratuity.ToString());
                Console.WriteLine("Has rated: {0}", ViewModel.HasRated.ToString());
                Console.WriteLine("Rating required: {0}", ViewModel.Settings.RatingRequired.ToString());
                Console.WriteLine("Rating enabled: {0}", ViewModel.Settings.RatingEnabled.ToString());
                Console.WriteLine();
		    };
            t.Start();

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