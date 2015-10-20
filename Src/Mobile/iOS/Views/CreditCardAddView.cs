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
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        private PayPalClientSettings _payPalSettings;
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
                    && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken)
                    && UIHelper.IsOS7orHigher; 
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

            if (!ViewModel.CanChooseTip)
            {
                viewTip.RemoveFromSuperview();
            }
            else
            {
                ConfigureTipSection();
            }

            if (!ViewModel.CanChooseLabel)
            {
                viewLabel.RemoveFromSuperview();
            }
            else
            {
                ConfigureLabelSection();
            }

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

            set.Bind(lblInstructions)
                .For(v => v.Text)
                .To(vm => vm.Instructions);

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
            
            set.Bind(btnCardDefault)
                .For("TouchUpInside")
                .To(vm => vm.SetAsDefault);
            
            set.Bind(btnCardDefault)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CanSetCreditCardAsDefault)
                .WithConversion("BoolInverter");
            
            set.Bind(txtNameOnCard)
				.For(v => v.Text)
				.To(vm => vm.Data.NameOnCard);

            set.Bind(txtZipCode)
                .For(v => v.Text)
                .To(vm => vm.Data.ZipCode);

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

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(segmentedLabel)
                .For(v => v.SelectedSegment)
                .To(vm => vm.Data.Label)
                .WithConversion("CreditCardLabel");

			set.Apply ();   

            txtNameOnCard.ShouldReturn += GoToNext;
        }

        private void ConfigureTipSection()
        {
            lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");

            txtTip.Placeholder = Localize.GetValue("PaymentDetails.TipAmountLabel");
            txtTip.AccessibilityLabel = txtTip.Placeholder;

            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id, true);
            txtTip.TextAlignment = UITextAlignment.Right;
        }

        private void ConfigureLabelSection()
        {
            lblLabel.Text = Localize.GetValue("PaymentDetails.LabelName");
            segmentedLabel.TintColor = UIColor.FromRGB(90, 90, 90);
            segmentedLabel.SetTitle(Localize.GetValue("PaymentDetails.Label." + CreditCardLabelConstants.Personal), 0);
            segmentedLabel.SetTitle(Localize.GetValue("PaymentDetails.Label." + CreditCardLabelConstants.Business), 1);
        }

        private void ConfigureCreditCardSection()
        {
            if (CardIOIsEnabled && ViewModel.CanScanCreditCard)
            {
                FlatButtonStyle.Silver.ApplyTo(btnScanCard);
                btnScanCard.SetTitle(Localize.GetValue("ScanCreditCard"), UIControlState.Normal);
                btnScanCard.TouchUpInside += (sender, e) => ScanCard();
            }
            else
            {
                btnScanCard.RemoveFromSuperview();
            }
            FlatButtonStyle.Silver.ApplyTo(btnCardDefault);
            // Configure CreditCard section
            FlatButtonStyle.Green.ApplyTo(btnSaveCard);
            FlatButtonStyle.Red.ApplyTo (btnDeleteCard);
            btnDeleteCard.SetTitle(Localize.GetValue("DeleteCreditCardTitle"), UIControlState.Normal);

            lblNameOnCard.Text = Localize.GetValue("CreditCardName");
            lblCardNumber.Text = Localize.GetValue("CreditCardNumber");
            lblExpMonth.Text = Localize.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Localize.GetValue("CreditCardExpYear");
            lblCvv.Text = Localize.GetValue("CreditCardCCV");
            lblZipCode.Text = Localize.GetValue("CreditCardZipCode");

            txtNameOnCard.AccessibilityLabel = Localize.GetValue("CreditCardName");
            txtNameOnCard.Placeholder = txtNameOnCard.AccessibilityLabel;

            txtZipCode.AccessibilityLabel = Localize.GetValue("CreditCardZipCode");
            txtZipCode.Placeholder = txtZipCode.AccessibilityLabel;

            txtCardNumber.AccessibilityLabel = Localize.GetValue("CreditCardNumber");
            txtCardNumber.Placeholder = txtCardNumber.AccessibilityLabel;

            txtExpMonth.AccessibilityLabel = Localize.GetValue("CreditCardExpMonth");
            txtExpMonth.Placeholder = txtExpMonth.AccessibilityLabel;

            txtExpYear.AccessibilityLabel = Localize.GetValue("CreditCardExpYear");
            txtExpYear.Placeholder = txtExpYear.AccessibilityLabel;

            txtCvv.AccessibilityLabel = Localize.GetValue("CreditCardCCV");
            txtCvv.Placeholder = txtCvv.AccessibilityLabel;

            txtCardNumber.ClearsOnBeginEditing = true;
            txtCardNumber.ShowCloseButtonOnKeyboard();
            txtCvv.ShowCloseButtonOnKeyboard();
            txtZipCode.ShowCloseButtonOnKeyboard();

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
            Mvx.Resolve<IPayPalConfigurationService>().InitializeService(_payPalSettings);
            
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

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // ugly fix for iOS 7 bug with horizontal scrolling
            // unlike iOS 8, the contentSize is a bit larger than the view, resulting in an undesired horizontal bounce
            if (UIHelper.IsOS7)
            {
                var scrollView = (UIScrollView)View.Subviews[0];
                if (scrollView.ContentSize.Width > UIScreen.MainScreen.Bounds.Width)
                {
                    scrollView.ContentSize = new CoreGraphics.CGSize(UIScreen.MainScreen.Bounds.Width, scrollView.ContentSize.Height);
                }
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
                _payPalPaymentDelegate = new PayPalDelegate(authCode => ViewModel.LinkPayPalAccount(authCode));
                _payPalPayment = new PayPalCustomFuturePaymentViewController((PayPalConfiguration)Mvx.Resolve<IPayPalConfigurationService>().GetConfiguration(), _payPalPaymentDelegate);
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
                    AutomaticallyAdjustsScrollViewInsets = false,
                    HideCardIOLogo = true,
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
            private readonly Action<string> _futurePaymentAuthorized;

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


