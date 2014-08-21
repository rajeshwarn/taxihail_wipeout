using Android.App;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "ConfirmPairActivity", Theme = "@style/MainTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ConfirmPairActivity : BaseBindingActivity<ConfirmPairViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_ConfirmPair);
        }
    }
}