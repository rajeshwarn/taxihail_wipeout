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

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class RideSettingsView : BaseViewController<RideSettingsViewModel>
    {
        #region Constructors

        public RideSettingsView () 
            : base(new MvxShowViewModelRequest<LocationDetailViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
              
        public RideSettingsView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public RideSettingsView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

#endregion
		
        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
			
            // Release any cached data, images, etc that aren't in use.
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
           
            scrollView.ContentSize = new SizeF (320, sgmtPercentOrValue.Frame.Bottom + 20);

            lblName.Text = Resources.GetValue ("RideSettingsName");
            lblPhone.Text = Resources.GetValue ("RideSettingsPhone");
            lblVehicleType.Text = Resources.GetValue ("RideSettingsVehiculeType");
            lblChargeType.Text = Resources.GetValue ("RideSettingsChargeType");
            lblPassword.Text = Resources.GetValue ("RideSettingsPassword");

            base.DismissKeyboardOnReturn (txtName, txtPhone, txtTipAmount);
            
            var button = new MonoTouch.UIKit.UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.SaveCommand.Execute ();
            });

            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue ("View_RideSettings");

            ((ModalTextField)pickerVehiculeType).Configure (Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.VehicleTypeId, x => {
                ViewModel.SetVehiculeType.Execute (x.Id);
            });

            ((ModalTextField)pickerChargeType).Configure (Resources.RideSettingsChargeType, ViewModel.Payments, ViewModel.ChargeTypeId, x => {
                ViewModel.SetChargeType.Execute (x.Id);
            });


            lblCreditCard.Text = Resources.GetValue ("PaymentDetails.CreditCardLabel");
            lblTipAmount.Text = Resources.GetValue ("PaymentDetails.TipAmountLabel");
            lblOptional.Text = Resources.GetValue ("PaymentDetails.Optional");
            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

            sgmtPercentOrValue.SelectedSegment = ViewModel.PaymentPreferences.IsTipInPercent ? 0 : 1;
            sgmtPercentOrValue.ValueChanged += HandleValueChanged;


            this.AddBindings (new Dictionary<object, string> (){
                { txtName, "{'Text': {'Path': 'Name'}}" },
                { txtPhone, "{'Text': {'Path': 'Phone'}}" },
                { pickerVehiculeType, "{'Text': {'Path': 'VehicleTypeName'}}" },
                { pickerChargeType, "{'Text': {'Path': 'ChargeTypeName'}}" },
                { txtPassword, "{'NavigateCommand': {'Path': 'NavigateToUpdatePassword'}}" },
                { txtTipAmount, "{'Text': {'Path': 'PaymentPreferences.Tip'}}" },
                { btnCreditCard, "{'Text': {'Path': 'PaymentPreferences.SelectedCreditCard.FriendlyName'}, 'Last4Digits': {'Path': 'PaymentPreferences.SelectedCreditCard.Last4Digits'}, 'CreditCardCompany': {'Path': 'PaymentPreferences.SelectedCreditCard.CreditCardCompany'}, 'NavigateCommand': {'Path': 'PaymentPreferences.NavigateToCreditCardsList'}}" }
            });



        }

        void HandleValueChanged (object sender, EventArgs e)
        {
            ViewModel.PaymentPreferences.IsTipInPercent = (sgmtPercentOrValue.SelectedSegment == 0);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            
            ((UINavigationController)ParentViewController).View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            View.BackgroundColor = UIColor.Clear; 

            if (!ViewModel.Settings.PayByCreditCardEnabled) {

                lblCreditCard.Hidden = true;
                lblTipAmount.Hidden = true;
                lblOptional.Hidden = true;
                sgmtPercentOrValue.Hidden = true;
                btnCreditCard.Hidden = true;
                txtTipAmount.Hidden = true;
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

