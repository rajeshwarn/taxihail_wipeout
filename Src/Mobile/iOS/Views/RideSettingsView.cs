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
            NavigationItem.Title = Localize.GetValue("View_RideSettings");
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(239, 239, 239);

			if (!ViewModel.ShouldDisplayCreditCards) {
                lblCreditCard.RemoveFromSuperview();//lblCreditCard.Hidden = true;           
                txtCreditCard.RemoveFromSuperview();//btnCreditCard.Hidden = true;
//				lblTipAmount.SetY (364);
//				TipSlider.SetY (393);
            }
//
//			if (!ViewModel.ShouldDisplayTipSlider) {
//				lblTipAmount.Hidden = true;
//				TipSlider.Hidden = true;
//			}
//                        
//			if (!ViewModel.ShouldDisplayTipSlider && !ViewModel.ShouldDisplayCreditCards) {
//				Container.SetBottom (lblChargeType.Frame.Bottom);
//			}

			lblName.Text= Localize.GetValue("RideSettingsName");
			lblPhone.Text= Localize.GetValue("RideSettingsPhone");
			lblVehicleType.Text= Localize.GetValue("RideSettingsVehiculeType");
			lblChargeType.Text= Localize.GetValue("RideSettingsChargeType");
			lblPassword.Text = Localize.GetValue("RideSettingsPassword");
            lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");
            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

            DismissKeyboardOnReturn(txtName, txtPhone);

            var button = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, delegate
            {
                ViewModel.SaveCommand.Execute();
            });
            NavigationItem.RightBarButtonItem = button;

            txtVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), ViewModel.Vehicles, ViewModel.VehicleTypeId, x => ViewModel.SetVehiculeType.Execute(x.Id));
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), ViewModel.Payments, ViewModel.ChargeTypeId, x => ViewModel.SetChargeType.Execute(x.Id));
//            txtTip.Configure("Tip", ViewModel.Payments, ViewModel.ChargeTypeId, x => ViewModel.SetChargeType.Execute(x.Id));

			var set = this.CreateBindingSet<RideSettingsView, RideSettingsViewModel> ();

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.Name);

			set.Bind(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Phone);

            set.Bind(txtVehicleType)
				.For("Text")
				.To(vm => vm.VehicleTypeName);

            set.Bind(txtChargeType)
				.For("Text")
				.To(vm => vm.ChargeTypeName);

			set.Bind(txtPassword)
				.For(v => v.NavigateCommand)
				.To(vm => vm.NavigateToUpdatePassword);

            set.Bind(txtCreditCard)
				.For("Text")
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

//			set.Bind(TipSlider)
//				.For("Value")
//				.To(vm => vm.PaymentPreferences.Tip);
//
			set.Apply ();       
        }
    }
}

