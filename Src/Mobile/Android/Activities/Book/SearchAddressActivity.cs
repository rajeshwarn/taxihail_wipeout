using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "SearchActivity", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SearchAddressActivity : BaseBindingActivity<AddressSearchViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SearchAddress; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_SearchAddress);


            var searchAddressText = FindViewById<EditText>(Resource.Id.searchAddressText);


            var button = FindViewById<ImageButton>(Resource.Id.clearable_button_clear);
            button.Touch += (sender, args) =>
            {
                searchAddressText.Text = "";
                ViewModel.ClearResults();
            };

            searchAddressText.TextChanged += delegate
            {
                if (searchAddressText.Text.Length > 0)
                {
                    button.Visibility = ViewStates.Visible;
                }
                else
                {
                    button.Visibility = ViewStates.Gone;
                }
            };

            if (searchAddressText.Length() > 0)
            {
                button.Visibility = ViewStates.Visible;
            }
        }
    }
}