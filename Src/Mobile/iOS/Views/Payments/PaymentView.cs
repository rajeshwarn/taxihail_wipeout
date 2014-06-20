using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class PaymentView : BaseViewController<PaymentViewModel>
    {
        public PaymentView() : base("PaymentView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBarHidden = false;
            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("View_Payment");
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			if (!ViewModel.PaymentSelectorToggleIsVisible) {
                payPalToggle.RemoveFromSuperview();
			}

            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip,  
                x => {
                    ViewModel.PaymentPreferences.Tip = (int)x.Id;
                    ViewModel.ToggleToTipSelector.Execute();
                });

            txtTip.TextAlignment = UITextAlignment.Right;

            ClearKeyboardButton.TouchUpInside += (sender, e) => {
                this.View.ResignFirstResponderOnSubviews();
            };

            lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");
			lblTip.Text = Localize.GetValue("PaymentViewTipText");
            lblTipAmount.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
            lblMeterAmount.Text = Localize.GetValue("PaymentViewMeterAmountText");
            lblTotal.Text = Localize.GetValue("PaymentViewTotalText");
            btnConfirm.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);

            FlatButtonStyle.Green.ApplyTo(btnConfirm); 

            btnConfirm.TouchDown += (sender, e) =>
			{
                if ( txtMeterAmount.IsFirstResponder )
				{
                    txtMeterAmount.ResignFirstResponder();
				}
                if ( txtTipAmount.IsFirstResponder )
				{
                    txtTipAmount.ResignFirstResponder();
				}
			};

            var set = this.CreateBindingSet<PaymentView, PaymentViewModel>();

            // Bindings for keyboard buttons

            set.Bind(ClearKeyboardButton)
                .For("TouchUpInside")
                .To(vm => vm.ShowCurrencyCommand);

            set.Bind(btnConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmOrderCommand);

            // Binding for Tip Picker Text

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmountDisplay);

            // Bindings to clear input when starting to edit, Started = Focus

            set.Bind(txtTipAmount)
                .For("Started")
                .To(vm => vm.ClearTipCommand);

            set.Bind(txtMeterAmount)
                .For("Started")
                .To(vm => vm.ClearMeterCommand);

            // Bindings to show currency sign when finishing to edit

            set.Bind(txtTipAmount)
                .For("Ended")
                .To(vm => vm.ShowCurrencyCommand);

            set.Bind(txtMeterAmount)
                .For("Ended")
                .To(vm => vm.ShowCurrencyCommand);

            // Half-duplex binding to distinguish value and display

            set.Bind(txtTipAmount)
                .For(v => v.Text)
                .To(vm => vm.TipAmount).OneWayToSource();

            set.Bind(txtTipAmount)
                .For(v => v.Text)
                .To(vm => vm.TipAmountString).OneWay();

            // Two-way bindings for Meter and Total

            set.Bind(txtMeterAmount)
                .For(v => v.Text)
                .To(vm => vm.MeterAmount);

            set.Bind(lblTotalValue)
                .For(v => v.Text)
                .To(vm => vm.TotalAmount);

            // Tip-Mode Togglers

            set.Bind(txtTipAmount)
                .For("Started")
                .To(vm => vm.ToggleToTipCustom); // See txtTip.Configure for the "Toggle to Tip Picker" command

            set.Bind(txtTipAmount)
				.For(v => v.Placeholder)
				.To(vm => vm.PlaceholderAmount);

            set.Bind(txtMeterAmount)
                .For(v => v.Placeholder)
                .To(vm => vm.PlaceholderAmount);

            // Other bindings in layout

			set.Bind(payPalToggle)
				.For(v => v.PayPalSelected)
				.To(vm => vm.PayPalSelected);

			set.Bind(payPalToggle)
				.For(v => v.Hidden)
				.To(vm => vm.PaymentSelectorToggleIsVisible)
				.WithConversion("BoolInverter");

            set.Bind(txtCreditCard)
                .For(v => v.Text)
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.FriendlyName);

            set.Bind(txtCreditCard)
				.For(v => v.Last4Digits)
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.Last4Digits);

            set.Bind(txtCreditCard)
				.For("CreditCardCompany")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.CreditCardCompany);

            set.Bind(txtCreditCard)
				.For(v => v.NavigateCommand)
				.To(vm => vm.PaymentPreferences.NavigateToCreditCardsList);

            set.Bind(txtCreditCard)
                .For(v => v.Hidden)
                .To(vm => vm.PayPalSelected);

            set.Bind(lblCreditCard)
                .For(v => v.Hidden)
                .To(vm => vm.PayPalSelected);

            set.Bind(imgPayPal)
                .For(v => v.Hidden)
                .To(vm => vm.PayPalSelected)
                .WithConversion("BoolInverter");

			set.Apply ();
        }
    }
}

