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
            Container.BackgroundColor =  UIColor.Clear;
            scrollView.BackgroundColor =UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

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


            var button = new UIBarButtonItem(Localize.GetValue("DoneButton"), UIBarButtonItemStyle.Plain, delegate
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

            this.AddBindings(new Dictionary<object, string>(){
                { txtName, new B("Text","Name") },
                { txtPhone, new B("Text","Phone") },
                { pickerVehiculeType, new B("Text","VehicleTypeName") },
                { pickerChargeType, new B("Text","ChargeTypeName") },
                { txtPassword, new B("NavigateCommand","NavigateToUpdatePassword") },
                { btnCreditCard, 
                    new B("Text","PaymentPreferences.SelectedCreditCard.FriendlyName")
                    .Add("Last4Digits","PaymentPreferences.SelectedCreditCard.Last4Digits")
                    .Add("CreditCardCompany","PaymentPreferences.SelectedCreditCard.CreditCardCompany")
                    .Add("NavigateCommand","PaymentPreferences.NavigateToCreditCardsList") },
                { TipSlider, new B("Value","PaymentPreferences.Tip",B.Mode.TwoWay) }
            });         

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);


        }

    }
}

