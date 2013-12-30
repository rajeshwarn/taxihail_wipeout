using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Dialog.Touch.Dialog;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class RideSettingsView : BaseViewController<RideSettingsViewModel>
    {              
        public RideSettingsView(MvxShowViewModelRequest request) 
            : base(request)
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

			lblName.Text= Resources.GetValue("RideSettingsName");
			lblPhone.Text= Resources.GetValue("RideSettingsPhone");
			lblVehicleType.Text= Resources.GetValue("RideSettingsVehiculeType");
			lblChargeType.Text= Resources.GetValue("RideSettingsChargeType");
			lblPassword.Text = Resources.GetValue("RideSettingsPassword");

            base.DismissKeyboardOnReturn(txtName, txtPhone);
            

            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.SaveCommand.Execute();
            });

            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue("View_RideSettings");

            ((ModalTextField)pickerVehiculeType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.VehicleTypeId, x=> {
                ViewModel.SetVehiculeType.Execute(x.Id);
            });

            ((ModalTextField)pickerChargeType).Configure(Resources.RideSettingsChargeType, ViewModel.Payments, ViewModel.ChargeTypeId, x=> {
                ViewModel.SetChargeType.Execute(x.Id);
            });



            lblCreditCard.Text = Resources.GetValue("PaymentDetails.CreditCardLabel");

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

