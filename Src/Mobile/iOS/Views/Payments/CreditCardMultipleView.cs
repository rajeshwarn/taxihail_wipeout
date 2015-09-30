using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

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

            tblCreditCards.DeselectRow(tblCreditCards.IndexPathForSelectedRow, false);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            tblCreditCards.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tblCreditCards.BackgroundColor = UIColor.Clear;
            tblCreditCards.SeparatorColor = UIColor.Clear;
            tblCreditCards.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tblCreditCards.AlwaysBounceVertical = false;
            tblCreditCards.DelaysContentTouches = false;

            var tableViewSource = new MvxSimpleTableViewSource(tblCreditCards, CreditCardCell.Key);
            tblCreditCards.Source = tableViewSource;
            tblCreditCards.RowHeight = 49;

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

            set.Bind(tableViewSource)
                .For(v => v.ItemsSource)
                .To(vm => vm.CreditCards);

            set.Bind(tableViewSource)
                .For(v => v.SelectionChangedCommand)
                .To(vm => vm.NavigateToDetails);
            
            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(btnAddCard)
                .To(vm => vm.NavigateToAddCard);
            
            set.Bind(btnAddCard)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CanAddCard)
                .WithConversion("BoolInverter");
            
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

