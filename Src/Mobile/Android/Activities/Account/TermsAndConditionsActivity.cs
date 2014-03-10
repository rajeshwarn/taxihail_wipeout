using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Terms and Conditions", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class TermsAndConditionsActivity : BaseBindingActivity<TermsAndConditionsViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_TermsAndConditions);

            var textView = FindViewById<TextView>(Resource.Id.TermsAndConditionsTextView);
            textView.MovementMethod = new ScrollingMovementMethod();
        }
    }
}