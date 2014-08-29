using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Card.IO;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
		private Card.IO.PaymentViewController  _cardScanner;

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

			if (PaymentViewController.CanReadCardWithCamera  && !string.IsNullOrWhiteSpace(this.Services().Settings.CardIOToken))
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
				.To(vm => vm.IsEditing)
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
				.To(vm => vm.ExpirationYear);

            set.Bind(txtCvv)
				.For(v => v.Text)
				.To(vm => vm.Data.CCV);

			set.Apply ();   

            txtNameOnCard.ShouldReturn += GoToNext;
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
				var cardScannerDelegate = new  PaymentViewControllerDelegate ();
				cardScannerDelegate.OnScanCompleted+= (PaymentViewController viewController, CreditCardInfo cardInfo) => 
				{
					_cardScanner.DismissViewController(true, () => {});
					if (cardInfo != null )
					{
                    PopulateCreditCardName(cardInfo);
					}
                };

				_cardScanner = new PaymentViewController(cardScannerDelegate)
                {
                    GuideColor = this.View.BackgroundColor,
                    SuppressScanConfirmation = true,
                    CollectCVV = false,
                    CollectExpiry = false,
                    DisableManualEntryButtons = true,
                    AppToken = this.Services().Settings.CardIOToken
                };
            }

            PresentViewController(_cardScanner, true, null);
        }

		private void PopulateCreditCardName(CreditCardInfo  info)
        {
			 
            txtCardNumber.Text = info.CardNumber;
			txtCvv.Text = info.Cvv;

			txtExpYear.Text = info.ExpiryYear.ToString ();
			txtExpMonth.Text = info.ExpiryMonth.ToString ();

            ViewModel.CreditCardNumber = info.CardNumber;
            txtCvv.BecomeFirstResponder();
        }
    }
}


