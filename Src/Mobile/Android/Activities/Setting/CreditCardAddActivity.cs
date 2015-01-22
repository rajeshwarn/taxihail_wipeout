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
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using Cirrious.CrossCore;
using Org.Json;
using PaypalSdkDroid.CardPayment;
using PaypalSdkDroid.Payments;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "CreditCardAddActivity", 
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class CreditCardAddActivity : BaseBindingActivity<CreditCardAddViewModel>
    {
        private Intent _scanIntent { get; set; }
        private const int CardIOScanRequestCode = 981288735; // TODO: Handle arbitrary number in a better way
        private const int LinkPayPalAccountRequestCode = 481516234;

        private ClientPaymentSettings _paymentSettings;
        private static readonly PayPalConfiguration PayPalConfiguration = new PayPalConfiguration();

        protected override async void OnViewModelSet()
		{
			base.OnViewModelSet ();

            ViewModel.CreditCardCompanies[0].Image = Resource.Drawable.visa.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[1].Image = Resource.Drawable.mastercard.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[2].Image = Resource.Drawable.amex.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[3].Image = Resource.Drawable.visa_electron.ToString(CultureInfo.InvariantCulture);
            ViewModel.CreditCardCompanies[4].Image = Resource.Drawable.credit_card_generic.ToString(CultureInfo.InvariantCulture);

            SetContentView(Resource.Layout.View_Payments_CreditCardAdd);
            
            var btnScanCard = FindViewById<Button>(Resource.Id.ScanCreditCardButton);

            if (CardIOActivity.CanReadCardWithCamera() && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken))
            {
                _scanIntent = new Intent(this, typeof(CardIOActivity));
                _scanIntent.PutExtra(CardIOActivity.ExtraAppToken, this.Services().Settings.CardIOToken);
                _scanIntent.PutExtra(CardIOActivity.ExtraRequireExpiry, false);
                _scanIntent.PutExtra(CardIOActivity.ExtraSuppressManualEntry, true);
                _scanIntent.PutExtra(CardIOActivity.ExtraSuppressConfirmation, true);   

                btnScanCard.Click += (sender, e) => ScanCard();

                btnScanCard.Visibility = ViewStates.Visible;
            }
            else
            {
                btnScanCard.Visibility = ViewStates.Gone;
            }

            _paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();

            SetUpPayPal();
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

        private void SetUpPayPal()
        {
            var mainScrollView = FindViewById<ScrollView>(Resource.Id.MainScrollView);
            var paypalSeparator = FindViewById<LinearLayout>(Resource.Id.PayPalSeparator);
            var btnLinkPayPalAccount = FindViewById<Button>(Resource.Id.LinkPayPalAccountButton);
            var btnUnlinkPayPalAccount = FindViewById<Button>(Resource.Id.UnLinkPayPalAccountButton);
            var btnPayPalOnlyLinkAccount = FindViewById<Button>(Resource.Id.PayPalOnlyLinkAccountButton);
            var btnPayPalOnlyUnlinkAccount = FindViewById<Button>(Resource.Id.PayPalOnlyUnlinkAccountButton);

            
            if (_paymentSettings.IsPayInTaxiEnabled)
            {
                btnPayPalOnlyLinkAccount.Visibility = ViewStates.Gone;
                btnPayPalOnlyUnlinkAccount.Visibility = ViewStates.Gone;
            }
            else
            {
                mainScrollView.Visibility = ViewStates.Gone;
            }

            // Use PayPal settings
            if (_paymentSettings.PayPalClientSettings.IsEnabled)
            {
                SetUpPayPalService(_paymentSettings.PayPalClientSettings);

                btnLinkPayPalAccount.Click += (sender, e) => LinkPayPayAccount();
                btnUnlinkPayPalAccount.Click += (sender, e) => ViewModel.UnLinkPayPalAccount();
                btnPayPalOnlyLinkAccount.Click += (sender, e) => LinkPayPayAccount();
                btnPayPalOnlyUnlinkAccount.Click += (sender, e) => ViewModel.UnLinkPayPalAccount();

            }
            else
            {
                // Paypal disabled
                paypalSeparator.Visibility = ViewStates.Gone;
                btnLinkPayPalAccount.Visibility = ViewStates.Gone;
                btnUnlinkPayPalAccount.Visibility = ViewStates.Gone;
                btnPayPalOnlyLinkAccount.Visibility = ViewStates.Gone;
                btnPayPalOnlyUnlinkAccount.Visibility = ViewStates.Gone;
            }
        }

        private void SetUpPayPalService(PayPalClientSettings paypalSettings)
        {
            PayPalConfiguration.LanguageOrLocale(this.Services().Localize.CurrentLanguage);
            PayPalConfiguration.AcceptCreditCards(false);

            PayPalConfiguration.Environment(paypalSettings.IsSandbox
                ? PayPalConfiguration.EnvironmentSandbox
                : PayPalConfiguration.EnvironmentProduction);

            PayPalConfiguration.ClientId(paypalSettings.IsSandbox
                ? paypalSettings.SandboxCredentials.ClientId
                : paypalSettings.Credentials.ClientId);

            PayPalConfiguration.MerchantName(ViewModel.Settings.TaxiHail.ApplicationName);
            PayPalConfiguration.MerchantPrivacyPolicyUri(
                Android.Net.Uri.Parse(string.Format("{0}/privacypolicy", ViewModel.Settings.ServiceUrl)));
            PayPalConfiguration.MerchantUserAgreementUri(
                Android.Net.Uri.Parse(string.Format("{0}/termsandconditions", ViewModel.Settings.ServiceUrl)));

            var intent = new Intent(this, typeof (PayPalService));
            intent.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfiguration);
            StartService(intent);
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
                        var intent = new Intent(this, typeof(PayPalFuturePaymentActivity));
                        StartActivityForResult(intent, LinkPayPalAccountRequestCode);
                    },
                    this.Services().Localize["Cancel"], () => { });
            }
        }

        private void ScanCard()
        {
            StartActivityForResult(_scanIntent, CardIOScanRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == CardIOScanRequestCode && data != null && data.HasExtra(CardIOActivity.ExtraScanResult))
            {
                var scanRes = data.GetParcelableExtra(CardIOActivity.ExtraScanResult);
                var scanResult = scanRes.JavaCast<CreditCard>();

                var txtCardNumber = FindViewById<EditTextLeftImage>(Resource.Id.CreditCardNumberEditText);
                ViewModel.Data.CardNumber = scanResult.CardNumber;
                txtCardNumber.CreditCardNumber = scanResult.CardNumber;
            }
            else if (requestCode == LinkPayPalAccountRequestCode && data != null)
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
                            ShowErrorDialog(e);
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

        private void ShowErrorDialog(JSONException ex)
        {
            Logger.LogError(ex);

            var alert = new AlertDialog.Builder(this);

            alert.SetTitle("Error");
            alert.SetMessage(ex.GetBaseException().Message);

            var input = new EditText(this);
            alert.SetView(input);

            alert.Create();
            alert.Show();
        }
    }
}