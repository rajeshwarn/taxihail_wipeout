using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using Card.IO;
using System;
using Foundation;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration.Impl;
using PaypalSdkTouch.Unified;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        private PayPalClientSettings _payPalSettings;
        private NSString _payPalEnvironment;

        private CardIOPaymentViewController _cardScanner;
        private CardScannerDelegate _cardScannerDelegate;

        private PayPalCustomFuturePaymentViewController _payPalPayment;
        private PayPalDelegate _payPalPaymentDelegate;

        private bool CardIOIsEnabled
        {
            get 
            { 
                // CardIOToken is only used to know if the company wants it or not
                return CardIOUtilities.CanReadCardWithCamera()
                    && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken); 
            }
        }

        private bool PayPalIsEnabled
        {
            get { return _payPalSettings.IsEnabled; }
        }

        public CreditCardAddView () : base("CreditCardAddView", null)
        {
        }
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                NavigationController.NavigationBar.Hidden = false;
                ChangeThemeOfBarStyle();
            }

			NavigationItem.HidesBackButton = ViewModel.IsMandatory;
            NavigationItem.Title = Localize.GetValue ("View_CreditCard");

            ChangeRightBarButtonFontToBold();

            if (CardIOIsEnabled)
            {
                CardIOUtilities.Preload();
            }
        }

        public override async void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            var paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();
            _payPalSettings = paymentSettings.PayPalClientSettings;

            lblInstructions.Text = Localize.GetValue("CreditCardInstructions");
            if (!ViewModel.ShowInstructions)
            {
                lblInstructions.RemoveFromSuperview();
            }
                
            if (!ViewModel.IsPayPalOnly)
            {
                ConfigureCreditCardSection();
            }
            else
            {
                viewCreditCard.RemoveFromSuperview();
            }

            if (PayPalIsEnabled)
            {
                ConfigurePayPalSection();
            }
            else
            {
                viewPayPal.RemoveFromSuperview();
            }

			var set = this.CreateBindingSet<CreditCardAddView, CreditCardAddViewModel>();

            set.Bind(btnSaveCard)
                .For("Title")
                .To(vm => vm.CreditCardSaveButtonDisplay);
            set.Bind(btnSaveCard)
                .For("TouchUpInside")
				.To(vm => vm.SaveCreditCardCommand);

			set.Bind(btnDeleteCard)
				.For("TouchUpInside")
				.To(vm => vm.DeleteCreditCardCommand);
			set.Bind(btnDeleteCard)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CanDeleteCreditCard)
				.WithConversion("BoolInverter");

            set.Bind(txtNameOnCard)
				.For(v => v.Text)
				.To(vm => vm.Data.NameOnCard);

			set.Bind(txtCardNumber)
				.For(v => v.Text)
				.To(vm => vm.CreditCardNumber);
			set.Bind(txtCardNumber)
				.For(v => v.ImageLeftSource)
				.To(vm => vm.CreditCardImagePath);

            set.Bind(txtExpMonth)
                .For(v => v.Text)
				.To(vm => vm.ExpirationMonthDisplay);

            set.Bind(txtExpYear)
                .For(v => v.Text)
				.To(vm => vm.ExpirationYearDisplay);

            set.Bind(txtCvv)
				.For(v => v.Text)
				.To(vm => vm.Data.CCV);

            set.Bind(btnLinkPayPal)
                .For(v => v.Hidden)
                .To(vm => vm.CanLinkPayPalAccount)
                .WithConversion("BoolInverter");

            set.Bind(btnUnlinkPayPal)
                .For(v => v.Hidden)
                .To(vm => vm.CanUnlinkPayPalAccount)
                .WithConversion("BoolInverter");

            set.Bind(viewPayPalIsLinkedInfo)
                .For(v => v.Hidden)
                .To(vm => vm.ShowLinkedPayPalInfo)
                .WithConversion("BoolInverter");

			set.Apply ();   

            txtNameOnCard.ShouldReturn += GoToNext;
        }

        private void ConfigureCreditCardSection()
        {
            if (CardIOIsEnabled)
            {
                FlatButtonStyle.Silver.ApplyTo(btnScanCard);
                btnScanCard.SetTitle(Localize.GetValue("ScanCreditCard"), UIControlState.Normal);
                btnScanCard.TouchUpInside += (sender, e) => ScanCard();
            }
            else
            {
                btnScanCard.RemoveFromSuperview();
            }

            // Configure CreditCard section
            FlatButtonStyle.Green.ApplyTo(btnSaveCard);
            FlatButtonStyle.Red.ApplyTo (btnDeleteCard);
            btnDeleteCard.SetTitle(Localize.GetValue("DeleteCreditCardTitle"), UIControlState.Normal);

            lblNameOnCard.Text = Localize.GetValue("CreditCardName");
            lblCardNumber.Text = Localize.GetValue("CreditCardNumber");
            lblExpMonth.Text = Localize.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Localize.GetValue("CreditCardExpYear");
            lblCvv.Text = Localize.GetValue("CreditCardCCV");

            txtCardNumber.ClearsOnBeginEditing = true;
            txtCardNumber.ShowCloseButtonOnKeyboard();
            txtCvv.ShowCloseButtonOnKeyboard();

            ViewModel.CreditCardCompanies[0].Image = "visa.png";
            ViewModel.CreditCardCompanies[1].Image = "mastercard.png";
            ViewModel.CreditCardCompanies[2].Image = "amex.png";
            ViewModel.CreditCardCompanies[3].Image = "visa_electron.png";
            ViewModel.CreditCardCompanies[4].Image = "credit_card_generic.png";

            txtExpMonth.Configure(Localize.GetValue("CreditCardExpMonth"), () => ViewModel.ExpirationMonths.ToArray(), () => ViewModel.ExpirationMonth, x => ViewModel.ExpirationMonth = x.Id);
            txtExpYear.Configure(Localize.GetValue("CreditCardExpYear"), () => ViewModel.ExpirationYears.ToArray(), () => ViewModel.ExpirationYear, x => ViewModel.ExpirationYear = x.Id);
        }

        private void ConfigurePayPalSection()
        {
            _payPalEnvironment = _payPalSettings.IsSandbox
                ? PayPalMobile.PayPalEnvironmentSandbox
                : PayPalMobile.PayPalEnvironmentProduction;

            PayPalMobile.WithClientIds(
                (NSString)_payPalSettings.Credentials.ClientId,
                (NSString)_payPalSettings.SandboxCredentials.ClientId);

            PayPalMobile.PreconnectWithEnvironment(_payPalEnvironment);

            lblPayPalLinkedInfo.Text = Localize.GetValue("PayPalLinkedInfo");

            FlatButtonStyle.Silver.ApplyTo(btnLinkPayPal);
			btnLinkPayPal.SetLeftImage("paypal_icon.png");
            btnLinkPayPal.SetTitle(Localize.GetValue("LinkPayPal"), UIControlState.Normal);
            btnLinkPayPal.TouchUpInside += (sender, e) => PayPalFlow();

            FlatButtonStyle.Silver.ApplyTo(btnUnlinkPayPal);
			btnLinkPayPal.SetLeftImage("paypal_icon.png");
            btnUnlinkPayPal.SetTitle(Localize.GetValue("UnlinkPayPal"), UIControlState.Normal);
            btnUnlinkPayPal.TouchUpInside += (sender, e) => ViewModel.UnlinkPayPalAccount();

            // Add horizontal separator
            if (!ViewModel.IsPayPalOnly)
            {
                viewPayPal.AddSubview(Line.CreateHorizontal(8f, 0f, viewPayPal.Frame.Width - (2*8f), UIColor.Black, 1f));
            }
        }

        private bool GoToNext (UITextField textField)
        {
            txtNameOnCard.ResignFirstResponder ();
            txtCardNumber.BecomeFirstResponder();
            return true;
        }

        private void PayPalFlow()
        {
            if (_payPalPayment == null)
            {
                _payPalPaymentDelegate = new PayPalDelegate(authCode => SendAuthCodeToServer(authCode));
                _payPalPayment = new PayPalCustomFuturePaymentViewController(new PayPalConfiguration
                {
                    AcceptCreditCards = false, 
                    LanguageOrLocale = (NSString)this.Services().Localize.CurrentLanguage,
                    MerchantName = (NSString)ViewModel.Settings.TaxiHail.ApplicationName,
                    MerchantPrivacyPolicyURL = new NSUrl(string.Format("{0}/privacypolicy", ViewModel.Settings.ServiceUrl)),
                    MerchantUserAgreementURL = new NSUrl(string.Format("{0}/termsandconditions", ViewModel.Settings.ServiceUrl)),
                    DisableBlurWhenBackgrounding = true
                }, _payPalPaymentDelegate);
            }

            if (ViewModel.IsEditing)
            {
                this.Services().Message.ShowMessage(
                    this.Services().Localize["DeleteCreditCardTitle"],
                    this.Services().Localize["LinkPayPalCCWarning"],
                    this.Services().Localize["LinkPayPalConfirmation"], () =>
                    {
                        PresentViewController(_payPalPayment, true, null);
                    },
                    this.Services().Localize["Cancel"], () => { });
            }
            else
            {
                PresentViewController(_payPalPayment, true, null);
            }
        }

        private void SendAuthCodeToServer(string authCode)
        {
            ViewModel.LinkPayPalAccount(authCode);
        }

        private void ScanCard ()
        {           
            if (_cardScanner == null)
            {
                _cardScannerDelegate = new CardScannerDelegate(cardInfo => PopulateCreditCardName(cardInfo));
                _cardScanner = new CardIOPaymentViewController(_cardScannerDelegate)
                {
                    GuideColor = this.View.BackgroundColor,
                    SuppressScanConfirmation = true,
                    CollectCVV = false,
                    CollectExpiry = false,
                    DisableManualEntryButtons = true,
                    DisableBlurWhenBackgrounding = true,
                    AutomaticallyAdjustsScrollViewInsets = false
                };
            }

            PresentViewController(_cardScanner, true, null);
        }

		private void PopulateCreditCardName(CardIOCreditCardInfo info)
        {
            txtCardNumber.Text = info.CardNumber;
            ViewModel.CreditCardNumber = info.CardNumber;
            txtCvv.BecomeFirstResponder();
        }

        private class CardScannerDelegate : CardIOPaymentViewControllerDelegate
        {
            private Action<CardIOCreditCardInfo> _cardScanned;

            public CardScannerDelegate (Action<CardIOCreditCardInfo> cardScanned)
            {
                _cardScanned = cardScanned;
            }

            public override void UserDidCancel(CardIOPaymentViewController paymentViewController)
            {
                paymentViewController.DismissViewController(true, null);
            }

            public override void UserDidProvideCreditCardInfo(CardIOCreditCardInfo cardInfo, CardIOPaymentViewController paymentViewController)
            {
                _cardScanned(cardInfo);
                paymentViewController.DismissViewController(true, null);
            }
        }

        private class PayPalDelegate : PayPalFuturePaymentDelegate
        {
            private Action<string> _futurePaymentAuthorized;

            public PayPalDelegate (Action<string> futurePaymentAuthorized)
            {
                _futurePaymentAuthorized = futurePaymentAuthorized;
            }

            public override void DidCancelFuturePayment(PayPalFuturePaymentViewController futurePaymentViewController)
            {
                Logger.LogMessage("PayPal LinkAccount: The user canceled the operation");
                futurePaymentViewController.DismissViewController(true, null);
            }

            public override void DidAuthorizeFuturePayment(PayPalFuturePaymentViewController futurePaymentViewController, NSDictionary futurePaymentAuthorization)
            {
                // The user has successfully logged into PayPal, and has consented to future payments.
                // Your code must now send the authorization response to your server.
                try
                {
                    NSError error;
                    var contentJSONData = NSJsonSerialization.Serialize(futurePaymentAuthorization, NSJsonWritingOptions.PrettyPrinted, out error);

                    if (error != null)
                    {
                        throw new Exception(error.LocalizedDescription + " " + error.LocalizedFailureReason);
                    }

                    var authResponse = JsonSerializer.DeserializeFromString<FuturePaymentAuthorization>(contentJSONData.ToString());
                    if (authResponse != null)
                    {
                        _futurePaymentAuthorized(authResponse.Response.Code);
                    }

                    // Be sure to dismiss the PayPalLoginViewController.
                    futurePaymentViewController.DismissViewController(true, null);
                }
                catch(Exception e)
                {
                    Logger.LogError(e);
                    Mvx.Resolve<IMessageService>().ShowMessage(Mvx.Resolve<ILocalization>()["Error"], e.GetBaseException().Message);
                }
            }
        }

        private class FuturePaymentAuthorization
        {
            public FuturePaymentAuthorization()
            {
                Response = new FuturePaymentAuthorizationResponse();
            }

            public FuturePaymentAuthorizationResponse Response { get; set; }

            public class FuturePaymentAuthorizationResponse
            {
                public string Code { get; set; }
            }
        }

        private class PayPalCustomFuturePaymentViewController : PayPalFuturePaymentViewController
        {
            public PayPalCustomFuturePaymentViewController(PayPalConfiguration configuration, PayPalFuturePaymentDelegate futurePaymentDelegate)
                : base(configuration, futurePaymentDelegate)
            {
            }

            public override void ViewWillAppear(bool animated)
            {
                base.ViewWillAppear(animated);

                // change navbar colors to PayPal light blue so we see it on the white background
                var payPalLightBlue = UIColor.FromRGB(40, 155, 228);
                ChangeNavBarButtonColor(payPalLightBlue);
            }

            public override void ViewWillDisappear(bool animated)
            {
                base.ViewWillDisappear(animated);

                // revert navbar colors
                ChangeNavBarButtonColor(Theme.LabelTextColor);
            }

            private void ChangeNavBarButtonColor(UIColor textColor)
            {
                var buttonFont = UIFont.FromName (FontName.HelveticaNeueLight, 34/2);

                // set back/left/right button color
                var buttonTextColor = new UITextAttributes 
                {
                    Font = buttonFont,
                    TextColor = textColor,
                    TextShadowColor = UIColor.Clear,
                    TextShadowOffset = new UIOffset(0,0)
                };
                var selectedButtonTextColor = new UITextAttributes
                {
                    Font = buttonFont,
                    TextColor = textColor.ColorWithAlpha(0.5f),
                    TextShadowColor = UIColor.Clear,
                    TextShadowOffset = new UIOffset(0,0)
                };

                UIBarButtonItem.Appearance.SetTitleTextAttributes(buttonTextColor, UIControlState.Normal);
                UIBarButtonItem.Appearance.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Highlighted);
                UIBarButtonItem.Appearance.SetTitleTextAttributes(selectedButtonTextColor, UIControlState.Selected);
            }
        }
    }
}


