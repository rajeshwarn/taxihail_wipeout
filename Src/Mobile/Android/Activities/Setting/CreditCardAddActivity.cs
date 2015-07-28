using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using Cirrious.CrossCore;
using Org.Json;
using PaypalSdkDroid.CardPayment;
using PaypalSdkDroid.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.OS;
using Android.Views.InputMethods;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "CreditCardAddActivity", 
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class CreditCardAddActivity : BaseBindingActivity<CreditCardAddViewModel>
    {
        private Intent _scanIntent { get; set; }
        private const int CardIOScanRequestCode = 981288735;
        private const int LinkPayPalAccountRequestCode = 481516234;

        private ClientPaymentSettings _paymentSettings;

        protected override async void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CreditCardAdd);

            _paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();

            ConfigureCreditCardSection();
            ConfigurePayPalSection();
		}

        protected override void OnDestroy()
        {
            if (_paymentSettings.PayPalClientSettings.IsEnabled)
            {
                // Stop service when done
                StopService(new Intent(this, typeof(PayPalService)));
            }
            
            base.OnDestroy();
        }

        private void ConfigureCreditCardSection()
        {
            ViewModel.CreditCardCompanies[0].Image = Resource.Drawable.visa.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[1].Image = Resource.Drawable.mastercard.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[2].Image = Resource.Drawable.amex.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[3].Image = Resource.Drawable.visa_electron.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[4].Image = Resource.Drawable.credit_card_generic.ToString(CultureInfo.InvariantCulture);

            var btnScanCard = FindViewById<Button>(Resource.Id.ScanCreditCardButton);

            var spinnerExpMonth = FindViewById<EditTextSpinner>(Resource.Id.ExpMonthSpinner);
            var spinnerExpYear = FindViewById<EditTextSpinner>(Resource.Id.ExpYearSpinner);

            spinnerExpMonth.OnTouch += (sender, e) => HideKeyboard(spinnerExpMonth.WindowToken);
            spinnerExpYear.OnTouch += (sender, e) => HideKeyboard(spinnerExpYear.WindowToken);

            if (CardIOActivity.CanReadCardWithCamera()
                // CardIOToken is only used to know if the company wants it or not
                && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken))
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

        private void ConfigurePayPalSection()
        {
            var paypalSeparator = FindViewById<LinearLayout>(Resource.Id.PayPalSeparator);
            var btnLinkPayPalAccount = FindViewById<Button>(Resource.Id.LinkPayPalAccountButton);
            var btnUnlinkPayPalAccount = FindViewById<Button>(Resource.Id.UnLinkPayPalAccountButton);

            // Use PayPal settings
            if (_paymentSettings.PayPalClientSettings.IsEnabled)
            {
                var payPalConfigurationService = Mvx.Resolve<IPayPalConfigurationService>();
                payPalConfigurationService.InitializeService(_paymentSettings.PayPalClientSettings);

                var intent = new Intent(this, typeof(PayPalService));
                intent.PutExtra(PayPalService.ExtraPaypalConfiguration, (PayPalConfiguration)payPalConfigurationService.GetConfiguration());
                StartService(intent);

                btnLinkPayPalAccount.Click += (sender, e) => LinkPayPayAccount();
                btnUnlinkPayPalAccount.Click += (sender, e) => ViewModel.UnlinkPayPalAccount();
            }
            else
            {
                // Paypal disabled
                paypalSeparator.Visibility = ViewStates.Gone;
            }
        }

        private void LinkPayPayAccount()
        {
            if (ViewModel.IsEditing)
            {
                this.Services().Message.ShowMessage(
                    this.Services().Localize["DeleteCreditCardTitle"],
                    this.Services().Localize["LinkPayPalCCWarning"],
                    this.Services().Localize["LinkPayPalConfirmation"], () =>
                    {
                        var intent = new Intent(this, typeof (PayPalFuturePaymentActivity));
                        StartActivityForResult(intent, LinkPayPalAccountRequestCode);
                    },
                    this.Services().Localize["Cancel"], () => { });
            }
            else
            {
                var intent = new Intent(this, typeof(PayPalFuturePaymentActivity));
                StartActivityForResult(intent, LinkPayPalAccountRequestCode);
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

            if (requestCode == CardIOScanRequestCode 
                && data.HasExtra(CardIOActivity.ExtraScanResult))
            {
                var scanRes = data.GetParcelableExtra(CardIOActivity.ExtraScanResult);
                var scanResult = scanRes.JavaCast<CreditCard>();

                var txtCardNumber = FindViewById<EditTextLeftImage>(Resource.Id.CreditCardNumberEditText);
                ViewModel.Data.CardNumber = scanResult.CardNumber;
                txtCardNumber.CreditCardNumber = scanResult.CardNumber;
            }
            else if (requestCode == LinkPayPalAccountRequestCode)
            {
                if (resultCode == Result.Ok)
                {
                    var rawAuthResponse = data.GetParcelableExtra(PayPalFuturePaymentActivity.ExtraResultAuthorization);
                    var authResponse = rawAuthResponse.JavaCast<PayPalAuthorization>();
                    if (authResponse != null)
                    {
                        try
                        {
                            ViewModel.LinkPayPalAccount(authResponse.AuthorizationCode);
                        }
                        catch (JSONException e)
                        {
                            Logger.LogError(e);
                            Mvx.Resolve<IMessageService>().ShowMessage(Mvx.Resolve<ILocalization>()["Error"], e.GetBaseException().Message);
                        }
                    }
                }
                else if (resultCode == Result.Canceled)
                {
                    Logger.LogMessage("PayPal LinkAccount: The user canceled the operation");
                }
                else if ((int)resultCode == PayPalFuturePaymentActivity.ResultExtrasInvalid)
                {
                    Logger.LogMessage("The attempt to previously start the PayPalService had an invalid PayPalConfiguration. Please see the docs.");
                }
            }
        }
    }
}