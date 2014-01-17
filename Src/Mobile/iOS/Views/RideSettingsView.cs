using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSettingsView : BaseViewController<RideSettingsViewModel>
    {              
        public RideSettingsView() 
			: base("RideSettingsView", null)
        {
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            NavigationController.NavigationBar.Hidden = false;
            Container.BackgroundColor = UIColor.Clear;
			scrollView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            View.BackgroundColor = UIColor.Clear; 

			if (!ViewModel.ShouldDisplayCreditCards) {
                lblCreditCard.Hidden = true;           
                btnCreditCard.Hidden = true;
				lblTipAmount.SetY (364);
				TipSlider.SetY (393);
            }

			if (!ViewModel.ShouldDisplayTipSlider) {
				lblTipAmount.Hidden = true;
				TipSlider.Hidden = true;
			}
                        
			if (!ViewModel.ShouldDisplayTipSlider && !ViewModel.ShouldDisplayCreditCards) {
				Container.SetBottom (lblChargeType.Frame.Bottom);
			}

            scrollView.AutoSize ();

			lblName.Text= Localize.GetValue("RideSettingsName");
			lblPhone.Text= Localize.GetValue("RideSettingsPhone");
			lblVehicleType.Text= Localize.GetValue("RideSettingsVehiculeType");
			lblChargeType.Text= Localize.GetValue("RideSettingsChargeType");
			lblPassword.Text = Localize.GetValue("RideSettingsPassword");

            DismissKeyboardOnReturn(txtName, txtPhone);

            var button = new UIBarButtonItem(Localize.GetValue("Done"), UIBarButtonItemStyle.Plain, delegate
            {
                ViewModel.SaveCommand.Execute();
            });

            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Localize.GetValue("View_RideSettings");

// ReSharper disable CoVariantArrayConversion
            (pickerVehiculeType).Configure(Localize.GetValue("RideSettingsVehiculeType"), ViewModel.Vehicles, ViewModel.VehicleTypeId, x => ViewModel.SetVehiculeType.Execute(x.Id));

            (pickerChargeType).Configure(Localize.GetValue("RideSettingsChargeType"), ViewModel.Payments, ViewModel.ChargeTypeId, x => ViewModel.SetChargeType.Execute(x.Id));
 // ReSharper restore CoVariantArrayConversion

            lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");

            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

			var set = this.CreateBindingSet<RideSettingsView, RideSettingsViewModel> ();

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.Name);

			set.Bind(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Phone);

			set.Bind(pickerVehiculeType)
				.For("Text")
				.To(vm => vm.VehicleTypeName);

			set.Bind(pickerChargeType)
				.For("Text")
				.To(vm => vm.ChargeTypeName);

			set.Bind(txtPassword)
				.For(v => v.NavigateCommand)
				.To(vm => vm.NavigateToUpdatePassword);

			set.Bind(btnCreditCard)
				.For("Text")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.FriendlyName);
			set.Bind(btnCreditCard)
				.For(v => v.Last4Digits)
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.Last4Digits);
			set.Bind(btnCreditCard)
				.For("CreditCardCompany")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.CreditCardCompany);
			set.Bind(btnCreditCard)
				.For(v => v.NavigateCommand)
				.To(vm => vm.PaymentPreferences.NavigateToCreditCardsList);

			set.Bind(TipSlider)
				.For("Value")
				.To(vm => vm.PaymentPreferences.Tip);

			set.Apply ();       
        }
    }
}

