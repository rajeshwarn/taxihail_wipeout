using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "ConfirmPaymentActivity", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConfirmPaymentActivity : BaseBindingActivity<ConfirmCarNumberViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Payments_ConfirmCarNumber);
            ViewModel.OnViewLoaded();
        }
    }
}
