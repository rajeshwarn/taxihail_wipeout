using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class CreditCardMultipleView : BaseViewController<CreditCardMultipleViewModel>
    {
        public CreditCardMultipleView()
            : base("CreditCardMultipleView", null)
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

            NavigationItem.Title = Localize.GetValue ("View_CreditCard");

            ChangeRightBarButtonFontToBold();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);
            tblCreditCards.BackgroundColor = View.BackgroundColor;

            if (!ViewModel.ShouldDisplayTip)
            {
                lblTip.RemoveFromSuperview();
                txtTip.RemoveFromSuperview();
            }
            else
            {
                ConfigureTipSection();
            }

            FlatButtonStyle.Green.ApplyTo(btnAddCard);

            var set = this.CreateBindingSet<CreditCardMultipleView, CreditCardMultipleViewModel>();

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(btnAddCard)
                .To(vm => vm.GoToAddCard);

            set.Apply ();  
        }

        private void ConfigureTipSection()
        {
            lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");

            txtTip.Placeholder = Localize.GetValue("PaymentDetails.TipAmountLabel");
            txtTip.AccessibilityLabel = txtTip.Placeholder;

            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id, true);
            txtTip.TextAlignment = UITextAlignment.Right;
        }
    }
}

