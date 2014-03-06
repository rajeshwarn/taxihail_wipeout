using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Terms and Conditions", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdatedTermsAndConditionsActivity : BaseBindingActivity<UpdatedTermsAndConditionsViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_UpdatedTermsAndConditions);

            var textView = FindViewById<TextView>(Resource.Id.UpdatedTermsAndConditionsTextView);
            textView.MovementMethod = new ScrollingMovementMethod();
        }
    }
}