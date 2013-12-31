using Android.App;
using Android.Content.PM;
using Android.Text;
using Android.Widget;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookStreetNumberActivity", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class BookStreetNumberActivity : BaseBindingActivity<BookStreetNumberViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.StreetNumberTitle; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookStreetNumber);
            var streetNumberText = FindViewById<EditText>(Resource.Id.streetNumberText);

            streetNumberText.SetFilters(new IInputFilter[]
            {new InputFilterLengthFilter(ViewModel.NumberOfCharAllowed)});

            streetNumberText.RequestFocus();
            streetNumberText.SelectAll();
        }
    }
}