using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "Password Recovery", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class PasswordRecoveryActivity : BaseBindingActivity<ResetPasswordViewModel>
    {
		protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_PasswordRecovery);
        }
    }
}