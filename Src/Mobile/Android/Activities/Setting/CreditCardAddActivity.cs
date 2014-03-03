using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using IO.Card.Payment;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "CreditCardAddActivity", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreditCardAddActivity : BaseBindingActivity<CreditCardAddViewModel>
    {
        // TODO: Get Api token from settings instead of activity, view or viewmodel
        public string CardIOToken = "fa9c4695da474a75b70ee86b75b28248";

        public int CardIOScanRequestCode = 981288735; // TODO: Handle arbitrary number in a better way

        private Intent _scanIntent { get; set; }

        protected override void OnViewModelSet()
        {
            ViewModel.CreditCardCompanies[0].Image = Resource.Drawable.visa.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[1].Image = Resource.Drawable.mastercard.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[2].Image = Resource.Drawable.amex.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[3].Image = Resource.Drawable.visa_electron.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[4].Image = Resource.Drawable.credit_card_generic.ToString(CultureInfo.InvariantCulture);
            SetContentView(Resource.Layout.View_Payments_CreditCardAdd);

            _scanIntent = new Intent(this, typeof(CardIOActivity));
            _scanIntent.PutExtra(CardIOActivity.ExtraAppToken, CardIOToken);
            _scanIntent.PutExtra(CardIOActivity.ExtraRequireExpiry, false); // default: true
            _scanIntent.PutExtra(CardIOActivity.ExtraSuppressManualEntry, true); // default: false
            _scanIntent.PutExtra(CardIOActivity.ExtraSuppressConfirmation, true);            
        }

        private void ShowCardIO()
        {
            StartActivityForResult(_scanIntent, CardIOScanRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == CardIOScanRequestCode && data != null && data.HasExtra(CardIOActivity.ExtraScanResult))
            {
                var scanRes = data.GetParcelableExtra(CardIOActivity.ExtraScanResult);
                CreditCard scanResult = scanRes.JavaCast<CreditCard>();                                
                ViewModel.Data.CardNumber = scanResult.CardNumber;                                
            }                        
        }
    }
}