using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using Card.IO;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
		private PaymentViewController _cardScanner;
        private CardScannerDelegate _cardScannerDelegate;

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

            PaymentViewController.Preload();
        }

        public override void ViewDidLoad ()
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

			if (PaymentViewController.CanReadCardWithCamera && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken))
            {
                FlatButtonStyle.Silver.ApplyTo(btnScanCard);
                btnScanCard.SetTitle(Localize.GetValue("ScanCreditCard"), UIControlState.Normal);
                btnScanCard.TouchUpInside += (sender, e) => ScanCard();
            }
            else
            {
                btnScanCard.RemoveFromSuperview();
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

        private void ScanCard ()
        {           
            if (_cardScanner == null)
            {
                _cardScannerDelegate = new CardScannerDelegate(cardInfo => PopulateCreditCardName(cardInfo));
                _cardScanner = new PaymentViewController(_cardScannerDelegate)
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

		private void PopulateCreditCardName(CreditCardInfo  info)
        {
            txtCardNumber.Text = info.CardNumber;
            ViewModel.CreditCardNumber = info.CardNumber;
            txtCvv.BecomeFirstResponder();
        }

        private class CardScannerDelegate : PaymentViewControllerDelegate
        {
            private Action<CreditCardInfo> _cardScanned;

            public CardScannerDelegate (Action<CreditCardInfo> cardScanned)
            {
                _cardScanned = cardScanned ;
            }

            public override void UserDidCancel(PaymentViewController paymentViewController)
            {
                paymentViewController.DismissViewController(true, null);
            }

            public override void UserDidProvideCreditCardInfo(CreditCardInfo cardInfo, PaymentViewController paymentViewController)
            {
                _cardScanned(cardInfo);
                paymentViewController.DismissViewController(true, null);
            }
        }
    }
}


