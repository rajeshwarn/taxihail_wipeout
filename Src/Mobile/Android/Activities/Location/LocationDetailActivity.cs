using System.Windows.Input;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
	[Activity(Label = "Location Details", 
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustResize
    )]
    public class LocationDetailActivity : BaseBindingActivity<LocationDetailViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_LocationDetail);
        }
    }
}