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

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        private PayPalClientSettings _payPalSettings;
        private NSString _payPalEnvironment;

        private CardIOPaymentViewController _cardScanner;
        private CardScannerDelegate _cardScannerDelegate;

        private PayPalFuturePaymentViewController _payPalPayment;
        private PayPalDelegate _payPalPaymentDelegate;

        private bool CardIOIsEnabled
        {
            get 
            { 
                // CardIOToken is only used to know if the user wants it or not
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

            NavigationController.NavigationBar.Hidden = false;
			NavigationItem.HidesBackButton = ViewModel.IsMandatory;
            NavigationItem.Title = Localize.GetValue ("View_CreditCard");

            ChangeThemeOfBarStyle();
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

			btnDeleteCard.SetTitle(Localize.GetValue("DeleteCreditCardTitle"), UIControlState.Normal);

            lblInstructions.Text = Localize.GetValue("CreditCardInstructions");
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

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, null);

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

            var paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();
            _payPalSettings = paymentSettings.PayPalClientSettings;

            if (PayPalIsEnabled)
            {
                _payPalEnvironment = _payPalSettings.IsSandbox
                    ? PayPalMobile.PayPalEnvironmentSandbox
                    : PayPalMobile.PayPalEnvironmentProduction;

                PayPalMobile.WithClientIds(
                    (NSString)_payPalSettings.Credentials.ClientId,
                    (NSString)_payPalSettings.SandboxCredentials.ClientId);

                PayPalMobile.PreconnectWithEnvironment(_payPalEnvironment);

                FlatButtonStyle.Silver.ApplyTo(btnPaypal);
                btnPaypal.SetTitle(Localize.GetValue("LinkPayPal"), UIControlState.Normal);
                btnPaypal.TouchUpInside += (sender, e) => PayPalFlow();
            }
            else
            {
                btnPaypal.RemoveFromSuperview();
            }
				
            if (!ViewModel.ShowInstructions)
            {
                lblInstructions.RemoveFromSuperview();
            }
				
			FlatButtonStyle.Red.ApplyTo (btnDeleteCard);

			var set = this.CreateBindingSet<CreditCardAddView, CreditCardAddViewModel>();

            set.Bind(NavigationItem.RightBarButtonItem)
                .For("Clicked")
				.To(vm => vm.SaveCreditCardCommand);

			set.Bind(btnDeleteCard)
				.For("TouchUpInside")
				.To(vm => vm.DeleteCreditCardCommand);

			set.Bind(btnDeleteCard)
				.For(v => v.Hidden)
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

			set.Apply ();   

            txtNameOnCard.ShouldReturn += GoToNext;

			ViewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "IsEditing")
				{
					NavigationItem.RightBarButtonItem.Title=ViewModel.CreditCardSaveButtonDisplay;
				}
			};
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
                _payPalPayment = new PayPalFuturePaymentViewController(new PayPalConfiguration
                {
                    AcceptCreditCards = false, 
                    LanguageOrLocale = (NSString)this.Services().Localize.CurrentLanguage,
                    MerchantName = (NSString)ViewModel.Settings.TaxiHail.ApplicationName,
                    MerchantPrivacyPolicyURL = new NSUrl(string.Format("{0}/privacypolicy", ViewModel.Settings.ServiceUrl)),
                    MerchantUserAgreementURL = new NSUrl(string.Format("{0}/termsandconditions", ViewModel.Settings.ServiceUrl)),
                    DisableBlurWhenBackgrounding = true,
                    PresentingInPopover = true
                }, _payPalPaymentDelegate);
            }

            PresentViewController(_payPalPayment, true, null);
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
                futurePaymentViewController.DismissViewController(true, null);
            }

            public override void DidAuthorizeFuturePayment(PayPalFuturePaymentViewController futurePaymentViewController, NSDictionary futurePaymentAuthorization)
            {
                // The user has successfully logged into PayPal, and has consented to future payments.
                // Your code must now send the authorization response to your server.
                var authCode = "";
                _futurePaymentAuthorized(authCode);

                // Be sure to dismiss the PayPalLoginViewController.
                futurePaymentViewController.DismissViewController(true, null);
            }
        }
    }
}


