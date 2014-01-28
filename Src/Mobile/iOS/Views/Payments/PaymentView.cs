using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class PaymentView : BaseViewController<PaymentViewModel>
    {
        public PaymentView() : base("PaymentView", null)
        {
        }

        double MeterAmount
        {
            get
            {
                return CultureProvider.ParseCurrency(txtMeterAmount.Text);
            }
            set
            {
                txtMeterAmount.Text = CultureProvider.FormatCurrency(value);
                ViewModel.MeterAmount = CultureProvider.FormatCurrency(value);//Todo ugly binding done in code behind
            }
        }

        double TipAmount
        {
            get
            {
                return CultureProvider.ParseCurrency(txtTipAmount.Text);
            }
            set
            {
                txtTipAmount.Text = CultureProvider.FormatCurrency(value);
                ViewModel.TipAmount = CultureProvider.FormatCurrency(value);//Todo ugly binding done in code behind
            }
        }

        double TotalAmount
        {
            get
            {
                return CultureProvider.ParseCurrency(lblTotalValue.Text);
            }
            set
            {
                lblTotalValue.Text = CultureProvider.FormatCurrency(value);
                ViewModel.TextAmount = lblTotalValue.Text; //Todo ugly binding done in code behind
            }
        }

        public void UpdateAmounts(bool hideKeyboard = true)
        {
            if(hideKeyboard)
            {
                View.ResignFirstResponderOnSubviews();
            }

            TotalAmount = TipAmount + MeterAmount;
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBarHidden = false;
            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("PaymentView");
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
                    TipAmount = MeterAmount * ((double)x.Id/100d);
                    UpdateAmounts(false);
                });
            txtTip.TextAlignment = UITextAlignment.Right;

            lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");
            lblTip.Text = Localize.GetValue("PaymentViewTipText");
            lblTipAmount.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
            lblMeterAmount.Text = Localize.GetValue("PaymentViewMeterAmountText");
            lblTotal.Text = Localize.GetValue("PaymentViewTotalText");
            btnConfirm.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);

            FlatButtonStyle.Green.ApplyTo(btnConfirm); 

            ClearKeyboardButton.TouchDown+= (sender, e) => {
                UpdateAmounts();
                MeterAmount = MeterAmount; //Format
                TipAmount = TipAmount; //Format
            };

            txtMeterAmount.ClearsOnBeginEditing = true;
            txtTipAmount.ClearsOnBeginEditing = true;

            txtTipAmount.EditingChanged+= (sender, e) => {
                UpdateAmounts(false);
            };

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

                ViewModel.MeterAmount = txtMeterAmount.Text;//Todo ugly binding done in code behind
                ViewModel.TipAmount = txtTipAmount.Text;//Todo ugly binding done in code behind
			};

            var set = this.CreateBindingSet<PaymentView, PaymentViewModel>();

            set.Bind(btnConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmOrderCommand);

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(txtMeterAmount)
				.For(v => v.Placeholder)
				.To(vm => vm.PlaceholderAmount);

            set.Bind(txtTipAmount)
				.For(v => v.Placeholder)
				.To(vm => vm.PlaceholderAmount);

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

