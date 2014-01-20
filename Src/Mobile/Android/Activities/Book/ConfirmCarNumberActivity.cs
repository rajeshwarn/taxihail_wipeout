using Android.App;
using Android.Content.PM;

using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "ConfirmCarNumberActivity", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConfirmCarNumberActivity : BaseBindingActivity<ConfirmCarNumberViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_PaymentCreditCardsOnFile; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Payments_ConfirmCarNumber);
            ViewModel.OnViewLoaded();
        }
    }
}