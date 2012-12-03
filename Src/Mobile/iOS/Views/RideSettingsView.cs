
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
           
            scrollView.ContentSize = new SizeF(320, 400);

            lblName.Text= Resources.GetValue("RideSettingsName");
            lblPassengers.Text= Resources.GetValue("RideSettingsPassengers");
            lblPhone.Text= Resources.GetValue("RideSettingsPhone");
            lblVehicleType.Text= Resources.GetValue("RideSettingsVehiculeType");
            lblChargeType.Text= Resources.GetValue("RideSettingsChargeType");
            base.DismissKeyboardOnReturn(txtName, txtPassengers, txtPhone);
            
            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.SaveCommand.Execute();
            });

            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue("View_RideSettings");

            ((ModalTextField)txtVehicleType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.VehicleTypeId, x=> {
                ViewModel.SetVehiculeType.Execute(x.Id);
            });
            ((ModalTextField)txtChargeType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Payments, ViewModel.ChargeTypeId, x=> {
                ViewModel.SetChargeType.Execute(x.Id);
            });

            AppButtons.FormatStandardButton(btnUpdatePassword, Resources.GetValue("View_RideSettings_BtUpdateMyPassword"), AppStyle.ButtonColor.Grey, null);

            this.AddBindings(new Dictionary<object, string>(){
                { txtName, "{'Text': {'Path': 'Name'}}" },
                { txtPhone, "{'Text': {'Path': 'Phone'}}" },
                { txtPassengers, "{'Text': {'Path': 'Passengers'}}" },
                { txtVehicleType, "{'Text': {'Path': 'VehicleTypeName'}}" },
                { txtChargeType, "{'Text': {'Path': 'ChargeTypeName'}}" },
                { btnUpdatePassword, "{'TouchUpInside': {'Path': 'NavigateToUpdatePassword'}}" }
            });

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            
            ((UINavigationController)ParentViewController).View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            View.BackgroundColor = UIColor.Clear; 
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

