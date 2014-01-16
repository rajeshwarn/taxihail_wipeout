using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Provider;
using Android.Views;
using CrossUI.Droid.Dialog;
using CrossUI.Droid;
using CrossUI.Core.Elements.Menu;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SignUpActivity : BaseBindingActivity<CreateAccountViewModel>
    {

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignUp; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_SignUp);

			EditText password = FindViewById<EditText>(Resource.Id.SignUpPassword);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);

			password = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword);
			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);



        }
    }
}