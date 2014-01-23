using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqChangePaymentView : BaseViewController<CmtRideLinqChangePaymentViewModel>
    {              
		public CmtRideLinqChangePaymentView() : base()
        {
        }    
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(239, 239, 239); 

            lblCardOnFile.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");
            lblTipAmount.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Localize.GetValue("Cancel"), UIBarButtonItemStyle.Plain, null);
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Done"), UIBarButtonItemStyle.Plain, null);

            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id);

			var set = this.CreateBindingSet<CmtRideLinqChangePaymentView, CmtRideLinqChangePaymentViewModel>();

            set.Bind(NavigationItem.LeftBarButtonItem)
                .For("Clicked")
                .To(vm => vm.CancelCommand);

            set.Bind(NavigationItem.RightBarButtonItem)
                .For("Clicked")
                .To(vm => vm.SaveCommand);

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

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

			set.Apply();    
        }
    }
}

