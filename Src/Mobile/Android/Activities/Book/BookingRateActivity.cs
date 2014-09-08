using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookingRateActivity", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookingRateActivity : BaseBindingActivity<BookRatingViewModel>
    {
        private ListView _listView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _listView = FindViewById<ListView>(Resource.Id.RatingListView);
            _listView.Divider = null;
            _listView.DividerHeight = 0;
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
                ViewModel.CheckAndSendRatings();

                if (!ViewModel.CanUserLeaveScreen())
                {
                    return false;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}