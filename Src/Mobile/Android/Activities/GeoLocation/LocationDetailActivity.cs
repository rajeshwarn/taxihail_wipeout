using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
	[Activity(Label = "Location Details", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationDetailActivity : BaseBindingActivity<LocationDetailViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_LocationDetail);
            FindViewById<EditText>(Resource.Id.LocationAddress).FocusChange += LocationDetailActivity_FocusChange;
        }

        private void LocationDetailActivity_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                ViewModel.ValidateAddress.Execute();
            }
        }
    }
}