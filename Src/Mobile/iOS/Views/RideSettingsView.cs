
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
    public partial class RideSettingsView : MvxBindingTouchViewController<RideSettingsViewModel>
    {
        CustomRootElement _vehiculeTypeEntry;

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

            ((ModalTextField)txtVehicleType).SetValues(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.VehicleTypeId, x=> {
                ViewModel.SetVehiculeType.Execute(x.Id);
            });

            this.AddBindings(new Dictionary<object, string>(){
                { txtVehicleType, "{'Text': {'Path': 'VehicleTypeName'}}" }
            });

        }

        void HandleVehicleTypeTouchDown (object sender, EventArgs e)
        {
            var newDvc =new DialogViewController (_vehiculeTypeEntry, true) {
                Autorotate = true
            };
            PresentViewController(newDvc,true, delegate {});
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

