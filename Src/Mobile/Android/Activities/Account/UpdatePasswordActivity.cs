using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "UpdatePasswordActivity", Theme = "@android:style/Theme.NoTitleBar",
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
        }
    }
}