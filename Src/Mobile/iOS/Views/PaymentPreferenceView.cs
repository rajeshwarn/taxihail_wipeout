
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class PaymentPreferenceView : BaseViewController<PaymentPreferenceViewModel>
    {
        #region Constructors
        
        public PaymentPreferenceView () 
            : base(new MvxShowViewModelRequest<PaymentPreferenceViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public PaymentPreferenceView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public PaymentPreferenceView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
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
            
            //scrollView.ContentSize = new SizeF(320, 400);
            
            txtCreditCards.Text= Resources.GetValue("PaymentCreditCardsOnFile");
            txtOptional.Text= Resources.GetValue("PaymentOptional");
            txtTipAmount.Text= Resources.GetValue("PaymentTipAmount");
            
            base.DismissKeyboardOnReturn(editTipAmount);
            
            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.ConfirmPreference.Execute();
            });
            
            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue("View_PaymentPreference");
            
            /*this.AddBindings(new Dictionary<object, string>(){
                { btnConfirm, "{'TouchUpInside':{'Path':'ConfirmPreference'}}"}
            });*/
            
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

