using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SignUpActivity : BaseBindingActivity<CreateAcccountViewModel>
    {

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignUp; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_SignUp);
        }
    }
}