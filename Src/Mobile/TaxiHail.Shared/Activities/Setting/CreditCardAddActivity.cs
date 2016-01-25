using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using Cirrious.CrossCore;
using Android.OS;
using Android.Views.InputMethods;
using Card.IO;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity(Label = "@string/CreditCardAddingActivityName", 
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class CreditCardAddActivity : BaseBindingActivity<CreditCardAddViewModel>
    {
        private Intent _scanIntent { get; set; }
        private const int CardIOScanRequestCode = 981288735;

        private ClientPaymentSettings _paymentSettings;

	    protected override async void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CreditCardAdd);

            _paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();

            ConfigureCreditCardSection();
		}

        private void ConfigureCreditCardSection()
        {
            ViewModel.CreditCardCompanies[0].Image = Resource.Drawable.visa.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[1].Image = Resource.Drawable.mastercard.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[2].Image = Resource.Drawable.amex.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[3].Image = Resource.Drawable.visa_electron.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[4].Image = Resource.Drawable.paypal_icon.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[5].Image = Resource.Drawable.credit_card_generic.ToString(CultureInfo.InvariantCulture);

            var btnScanCard = FindViewById<Button>(Resource.Id.ScanCreditCardButton);

            var spinnerExpMonth = FindViewById<EditTextSpinner>(Resource.Id.ExpMonthSpinner);
            var spinnerExpYear = FindViewById<EditTextSpinner>(Resource.Id.ExpYearSpinner);

            spinnerExpMonth.OnTouch += (sender, e) => HideKeyboard(spinnerExpMonth.WindowToken);
            spinnerExpYear.OnTouch += (sender, e) => HideKeyboard(spinnerExpYear.WindowToken);

            if (CardIOActivity.CanReadCardWithCamera()
                // CardIOToken is only used to know if the company wants it or not
                && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken) && ViewModel.CanScanCreditCard)
            {
                _scanIntent = new Intent(this, typeof(CardIOActivity));
                _scanIntent.PutExtra(CardIOActivity.ExtraRequireExpiry, false);
                _scanIntent.PutExtra(CardIOActivity.ExtraHideCardioLogo, true);
                _scanIntent.PutExtra(CardIOActivity.ExtraSuppressManualEntry, true);
                _scanIntent.PutExtra(CardIOActivity.ExtraSuppressConfirmation, true);

                btnScanCard.Click += (sender, e) => ScanCard();
                btnScanCard.Visibility = ViewStates.Visible;
            }
            else
            {
                btnScanCard.Visibility = ViewStates.Gone; 
            }
        }

        private void HideKeyboard(IBinder windowToken)
        {
            var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            if (inputManager != null)
            {
                inputManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.NotAlways);
            }
        }

        private void ScanCard()
        {
            StartActivityForResult(_scanIntent, CardIOScanRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            if (data == null)
            {
                return;
            }

            if (requestCode == CardIOScanRequestCode && data.HasExtra(CardIOActivity.ExtraScanResult))
            {
                var scanRes = data.GetParcelableExtra(CardIOActivity.ExtraScanResult);
                var scanResult = scanRes.JavaCast<CreditCard>();

                var txtCardNumber = FindViewById<EditTextLeftImage>(Resource.Id.CreditCardNumberEditText);
                ViewModel.Data.CardNumber = scanResult.CardNumber;
                txtCardNumber.CreditCardNumber = scanResult.CardNumber;
            }
        }
    }
}