using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity(Label = "RideSettingsActivity", 
        Theme = "@style/MainTheme",
        WindowSoftInputMode = SoftInput.AdjustResize, 
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class RideSettingsActivity : BaseBindingActivity<RideSettingsViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_RideSettings);
            var txtPassword = FindViewById<EditTextNavigate>(Resource.Id.rideSettingsPassword);
            txtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
            txtPassword.Text = "my secure password"; /* fake password for display only */

            if (!ViewModel.ShouldDisplayTip)
            {
                FindViewById<TextView>(Resource.Id.tipAmountLabel).Visibility = ViewStates.Gone;
                FindViewById<EditTextRightSpinner>(Resource.Id.tipPicker).Visibility = ViewStates.Gone;
            }
        }
    }
}