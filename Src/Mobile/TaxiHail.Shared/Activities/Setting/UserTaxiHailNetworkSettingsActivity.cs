using Android.App;
using Android.Content.PM;
using Android.OS;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity(Label = "@string/UserTaxiHailNetworkSettingsActivityName",
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UserTaxiHailNetworkSettingsActivity : BaseBindingActivity<UserTaxiHailNetworkSettingsViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_UserTaxiHailNetworkSettings);
        }
    }
}