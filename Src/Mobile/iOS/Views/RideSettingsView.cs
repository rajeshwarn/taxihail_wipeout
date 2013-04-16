using System;
using System.Drawing;
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

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
			
            // Release any cached data, images, etc that aren't in use.
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

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

            ((ModalTextField)pickerVehiculeType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.VehicleTypeId.Value, x=> {

                ViewModel.SetVehiculeType.Execute(x.Id);
            });

            ((ModalTextField)pickerChargeType).Configure(Resources.RideSettingsChargeType, ViewModel.Payments, ViewModel.ChargeTypeId.Value, x=> {
                ViewModel.SetChargeType.Execute(x.Id);
            });



            lblCreditCard.Text = Resources.GetValue("PaymentDetails.CreditCardLabel");

            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

            tipSlider.ValueChanged+= (sender, e) => {
                tipSlider.Value = (int)(Math.Round(tipSlider.Value/5.0)*5);
            };


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
                { tipSlider, 
                    new B("Value","PaymentPreferences.Tip",B.Mode.TwoWay) }
            });         

        }

        void HandleValueChanged (object sender, TipButtonsValueChangedEventArgs e)
        {
           // ViewModel.PaymentPreferences.IsTipInPercent = (e.ButtonIndex == 0);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            
            ((UINavigationController)ParentViewController).View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            View.BackgroundColor = UIColor.Clear; 

            if (!ViewModel.Settings.PayByCreditCardEnabled) {
                
                lblCreditCard.Hidden = true;
                tipSlider.Hidden = true;
                lblOptional.Hidden = true;
                btnCreditCard.Hidden = true;
            }
        }

		
        public override void ViewDidUnload ()
        {
            base.ViewDidUnload ();
            			
            // Clear any references to subviews of the main view in order to
            // allow the Garbage Collector to collect them sooner.
            //
            // e.g. myOutlet.Dispose (); myOutlet = null;
			
            ReleaseDesignerOutlets ();
        }
		
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
        {
            // Return true for supported orientations
            return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
        }

    }
}

