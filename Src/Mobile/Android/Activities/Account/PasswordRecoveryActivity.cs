using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Password Recovery", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class PasswordRecoveryActivity : BaseBindingActivity<ResetPasswordViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_PasswordRecovery_Label; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_PasswordRecovery);
        }
    }
}