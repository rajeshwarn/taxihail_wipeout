using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class BookDetailActivity : BaseBindingActivity<BookConfirmationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }


        protected override void OnViewModelSet()
        {            
            SetContentView(Resource.Layout.View_BookingDetail);
            FindViewById<LinearLayout>(Resource.Id.passengerNameLayout).Visibility = ViewModel.ShowPassengerName  ? ViewStates.Visible : ViewStates.Gone;
            FindViewById<LinearLayout>(Resource.Id.passengerPhoneLayout).Visibility = ViewModel.ShowPassengerPhone ? ViewStates.Visible : ViewStates.Gone;
            FindViewById<LinearLayout>(Resource.Id.passengerNumberLayout).Visibility = ViewModel.ShowPassengerNumber ? ViewStates.Visible : ViewStates.Gone;
			ViewModel.Load();
        }
    }
}
