using Android.App;
using Android.Content.PM;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "RideSettingsActivity", Theme = "@android:style/Theme.NoTitleBar",
        WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = ScreenOrientation.Portrait)]
    public class RideSettingsActivity : BaseBindingActivity<RideSettingsViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RideSettings; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_RideSettings);
            var txtPassword = FindViewById<EditTextNavigate>(Resource.Id.rideSettingsPassword);
            txtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
            txtPassword.Text = "my secure password"; /* fake password for display only */

            if (!ViewModel.ShouldDisplayCreditCards)
            {
                FindViewById<TextView>(Resource.Id.lblCC).Visibility = ViewStates.Gone;
                FindViewById<CreditCardButton>(Resource.Id.btCC).Visibility = ViewStates.Gone;
            }

            if (!ViewModel.ShouldDisplayTipSlider)
            {
                FindViewById<TextView>(Resource.Id.tipAmountLabel).Visibility = ViewStates.Gone;
                FindViewById<TipSlider>(Resource.Id.tipSlider).Visibility = ViewStates.Gone;
            }
        }
    }
}