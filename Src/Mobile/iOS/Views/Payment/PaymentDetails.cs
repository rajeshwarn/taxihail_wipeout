using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payment
{
    public partial class PaymentDetails : MvxBindingTouchViewController<PaymentDetailsViewModel>
    {
        public PaymentDetails () 
            : base(new MvxShowViewModelRequest<PaymentDetailsViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public PaymentDetails (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public PaymentDetails (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
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
			
            // Perform any additional setup after loading the view, typically from a nib.
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

