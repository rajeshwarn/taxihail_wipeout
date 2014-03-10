using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Terms and Conditions", Theme = "@style/LoginTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdatedTermsAndConditionsActivity : BaseBindingActivity<UpdatedTermsAndConditionsViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_UpdatedTermsAndConditions);

            var textView = FindViewById<TextView>(Resource.Id.UpdatedTermsAndConditionsTextView);
            textView.MovementMethod = new ScrollingMovementMethod();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                return false;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}