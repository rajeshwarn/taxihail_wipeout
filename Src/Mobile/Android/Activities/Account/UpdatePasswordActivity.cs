using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "UpdatePasswordActivity", Theme = "@style/OnlineTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdatePasswordActivity : BaseBindingActivity<UpdatePasswordViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_UpdatePassword; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_UpdatePassword);

			// There is no other way to clean the typeface for password hint
			// http://stackoverflow.com/questions/3406534/password-hint-font-in-android

			EditText password = FindViewById<EditText>(Resource.Id.txtPasswordCurrent);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

			password = FindViewById<EditText>(Resource.Id.txtPasswordNew);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

			password = FindViewById<EditText>(Resource.Id.txtPasswordNewConfirm);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

        }
    }
}