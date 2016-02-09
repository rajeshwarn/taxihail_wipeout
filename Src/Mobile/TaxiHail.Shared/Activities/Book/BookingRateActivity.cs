using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.Views;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "@string/BookingRateActivityName", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookingRateActivity : BaseBindingActivity<BookRatingViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
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

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
            SetContentView(Resource.Layout.View_BookingRating);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                var task = ViewModel.CheckAndSendRatings();
                task.Wait();

                if (!ViewModel.CanUserLeaveScreen())
                {
                    return false;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}