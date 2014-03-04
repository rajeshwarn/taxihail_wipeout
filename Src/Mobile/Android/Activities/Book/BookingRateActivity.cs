using Android.App;
using Android.Content.PM;
using Android.OS;
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


            if (Helpers.PlatformHelper.APILevel > 10)
            {
                _listView.SetPadding(10, 0, 10, 0);
            }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingRating);
        }
    }
}