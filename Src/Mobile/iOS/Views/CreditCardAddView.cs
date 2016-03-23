using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using Card.IO;
using System;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Common;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        private CardIOPaymentViewController _cardScanner;
        private CardScannerDelegate _cardScannerDelegate;

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

            Utilities.Preload();
        }

        public override async void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            var paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();

            lblInstructions.Text = Localize.GetValue("CreditCardInstructions");

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

            ConfigureCreditCardSection();

			if (paymentSettings.PaymentMode == PaymentMethod.Braintree && paymentSettings.BraintreeClientSettings.EnablePayPal)
            {
                ConfigurePayPalSection();
            }
            else
            {
                viewPayPal.RemoveFromSuperview();
				viewPayPalDetails.RemoveFromSuperview();
            }

			var set = this.CreateBindingSet<CreditCardAddView, CreditCardAddViewModel>();

            set.Bind(btnSaveCard)
                .For("Title")
                .To(vm => vm.CreditCardSaveButtonDisplay);

			set.Bind(btnSaveCard)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsAddingNewCard)
				.WithConversion("BoolInverter");

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

            set.Bind(btnScanCard)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CanScanCreditCard)
                .WithConversion("BoolInverter");

			set.Bind(viewCreditCard)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsShowPaypalView);
            
			set.Bind(viewPayPalDetails)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsShowPaypalView)
				.WithConversion("BoolInverter");

            set.Bind(txtNameOnCard)
				.For(v => v.Text)
				.To(vm => vm.Data.NameOnCard);

			set.Bind(txtPayPalAccountName)
				.For(v => v.Text)
				.To(vm => vm.Data.NameOnCard);
			
			set.Bind(txtNameOnCard)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

            set.Bind(txtZipCode)
                .For(v => v.Text)
                .To(vm => vm.Data.ZipCode);

			set.Bind(txtZipCode)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

			set.Bind(txtCardNumber)
				.For(v => v.Text)
				.To(vm => vm.CreditCardNumber);
            
			set.Bind(txtCardNumber)
				.For(v => v.ImageLeftSource)
				.To(vm => vm.CreditCardImagePath);

			set.Bind(txtCardNumber)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

            set.Bind(txtExpMonth)
                .For(v => v.Text)
				.To(vm => vm.ExpirationMonthDisplay);

			set.Bind(txtExpMonth)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

			set.Bind(txtExpMonth)
				.For(v => v.HasRightArrow)
				.To(vm => vm.IsAddingNewCard);

            set.Bind(txtExpYear)
                .For(v => v.Text)
				.To(vm => vm.ExpirationYearDisplay);

			set.Bind(txtExpYear)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

			set.Bind(txtExpYear)
				.For(v => v.HasRightArrow)
				.To(vm => vm.IsAddingNewCard);

            set.Bind(txtCvv)
				.For(v => v.Text)
				.To(vm => vm.Data.CCV);

			set.Bind(txtCvv)
				.For(v => v.Enabled)
				.To(vm => vm.IsAddingNewCard);

            set.Bind(btnLinkPayPal)
                .For(v => v.Hidden)
                .To(vm => vm.IsPaypalEnabled)
                .WithConversion("BoolInverter");

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(segmentedLabel)
                .For(v => v.SelectedSegment)
                .To(vm => vm.Data.Label)
                .WithConversion("CreditCardLabel");

			set.Bind(segmentedLabel)
				.For(v => v.Enabled)
                .To(vm => vm.IsAddingNewCard);

            set.Bind(imgVisa)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.PaymentSettings.DisableVisaMastercard);

            set.Bind(imgAmex)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.PaymentSettings.DisableAMEX);

            set.Bind(imgDiscover)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.PaymentSettings.DisableDiscover);

			set.Apply ();   

            txtNameOnCard.ShouldReturn += GoToNext;
        }

		private void ConfigurePayPalSection()
		{
			FlatButtonStyle.Silver.ApplyTo(btnLinkPayPal);
			btnLinkPayPal.SetLeftImage("paypal_icon.png");
			btnLinkPayPal.SetTitle(Localize.GetValue("LinkPayPal"), UIControlState.Normal);
			btnLinkPayPal.TouchUpInside += (s, e) =>
			{
				ViewModel.UsePaypalCommand.ExecuteIfPossible();
			};
			
			lblPayPalAccountName.Text = Localize.GetValue("PaypalAccount");
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
            FlatButtonStyle.Silver.ApplyTo(btnScanCard);
            btnScanCard.SetTitle(Localize.GetValue("ScanCreditCard"), UIControlState.Normal);
            btnScanCard.TouchUpInside += (sender, e) => ScanCard();

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
			ViewModel.CreditCardCompanies[4].Image = "paypal_icon.png";
            ViewModel.CreditCardCompanies[5].Image = "discover.png";
            ViewModel.CreditCardCompanies[6].Image = "credit_card_generic.png";

            txtExpMonth.Configure(Localize.GetValue("CreditCardExpMonth"), () => ViewModel.ExpirationMonths.ToArray(), () => ViewModel.ExpirationMonth, x => ViewModel.ExpirationMonth = x.Id);
            txtExpYear.Configure(Localize.GetValue("CreditCardExpYear"), () => ViewModel.ExpirationYears.ToArray(), () => ViewModel.ExpirationYear, x => ViewModel.ExpirationYear = x.Id);
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

        private void ScanCard ()
        {           
            if (_cardScanner == null)
            {
                _cardScannerDelegate = new CardScannerDelegate(PopulateCreditCardName);
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

		private void PopulateCreditCardName(CreditCardInfo info)
        {
            txtCardNumber.Text = info.CardNumber;
            ViewModel.CreditCardNumber = info.CardNumber;
            txtCvv.BecomeFirstResponder();
        }

        private class CardScannerDelegate : CardIOPaymentViewControllerDelegate
        {
            private Action<CreditCardInfo> _cardScanned;

            public CardScannerDelegate (Action<CreditCardInfo> cardScanned)
            {
                _cardScanned = cardScanned;
            }

			public override void UserDidCancelPaymentViewController(CardIOPaymentViewController paymentViewController)
			{
				paymentViewController.DismissViewController(true, null);
			}

            public override void UserDidProvideCreditCardInfo(CreditCardInfo cardInfo, CardIOPaymentViewController paymentViewController)
            {
                _cardScanned(cardInfo);
                paymentViewController.DismissViewController(true, null);
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
    }
}


