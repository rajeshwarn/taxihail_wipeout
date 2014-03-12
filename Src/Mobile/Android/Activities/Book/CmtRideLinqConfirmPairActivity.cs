using Android.App;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "CmtRideLinqConfirmPairActivity", Theme = "@style/MainTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CmtRideLinqConfirmPairActivity : BaseBindingActivity<CmtRideLinqConfirmPairViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CmtRideLinqConfirmPair);
        }
    }
}