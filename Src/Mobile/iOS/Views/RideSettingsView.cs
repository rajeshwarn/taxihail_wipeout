using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSettingsView : BaseViewController<RideSettingsViewModel>
    {              
        public RideSettingsView() 
			: base("RideSettingsView", null)
        {
        }
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue("RideSettingsView");
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(239, 239, 239);

			if (!ViewModel.ShouldDisplayCreditCards) {
                lblCreditCard.RemoveFromSuperview();
                txtCreditCard.RemoveFromSuperview();
                constraintContentViewHeight.Constant -= 80;
            }

			if (!ViewModel.ShouldDisplayTip) {
                lblTip.RemoveFromSuperview();
                txtTip.RemoveFromSuperview();
                constraintContentViewHeight.Constant -= 80;
			}

            lblName.Text = Localize.GetValue("RideSettingsName");
            lblPhone.Text = Localize.GetValue("RideSettingsPhone");
            lblVehicleType.Text = Localize.GetValue("RideSettingsVehiculeType");
            lblChargeType.Text = Localize.GetValue("RideSettingsChargeType");
			lblPassword.Text = Localize.GetValue("RideSettingsPassword");
            lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");
            lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

            DismissKeyboardOnReturn(txtName, txtPhone);

            txtVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), () => ViewModel.Vehicles, () => ViewModel.VehicleTypeId, x => ViewModel.SetVehiculeType.Execute(x.Id));
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), () => ViewModel.Payments, () => ViewModel.ChargeTypeId, x => ViewModel.SetChargeType.Execute(x.Id));
            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, null);

			var set = this.CreateBindingSet<RideSettingsView, RideSettingsViewModel> ();

            set.Bind (NavigationItem.RightBarButtonItem)
                .For ("Clicked")
                .To(vm => vm.SaveCommand);

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.Name);

			set.Bind(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Phone);

            set.Bind(txtVehicleType)
                .For(v => v.Text)
				.To(vm => vm.VehicleTypeName);

            set.Bind(txtChargeType)
                .For(v => v.Text)
				.To(vm => vm.ChargeTypeName);

			set.Bind(txtPassword)
				.For(v => v.NavigateCommand)
				.To(vm => vm.NavigateToUpdatePassword);

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

			set.Apply ();       
        }
    }
}

