using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Terms and Conditions", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class TermsAndConditionsActivity : BaseBindingActivity<TermsAndConditionsViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_TermsAndConditions; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_TermsAndConditions);

            var textView = FindViewById<TextView>(Resource.Id.TermsAndConditionsTextView);
            textView.MovementMethod = new ScrollingMovementMethod();
        }
    }
}