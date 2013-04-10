using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Threading.Tasks;
using System.Threading;

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

			FindViewById<EditText>(Resource.Id.noteEditText).FocusChange += HandleFocusChange;
			ViewModel.Load();
        }

        void HandleFocusChange (object sender, View.FocusChangeEventArgs e)
        {
			if (e.HasFocus) {
		
				Task.Factory.StartNew ( () =>
				                       {
					Thread.Sleep( 300 );
				 	RunOnUiThread( () => FindViewById<ScrollView> (Resource.Id.mainScroll).FullScroll (FocusSearchDirection.Down) );
				});
			}
		
        }
    }
}
