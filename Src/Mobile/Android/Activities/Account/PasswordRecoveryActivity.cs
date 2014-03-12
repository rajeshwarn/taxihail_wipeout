using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "Password Recovery", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class PasswordRecoveryActivity : BaseBindingActivity<ResetPasswordViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_PasswordRecovery);

            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.btnReset));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.btnCancel));
            DrawHelper.SupportLoginTextColor(FindViewById<TextView>(Resource.Id.lblSubtitle));
            DrawHelper.SupportLoginTextColor(FindViewById<TextView>(Resource.Id.lblTitle));
        }
    }
}