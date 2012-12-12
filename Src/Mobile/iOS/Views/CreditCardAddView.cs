
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        #region Constructors
        
        public CreditCardAddView () 
            : base(new MvxShowViewModelRequest<CreditCardAddViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public CreditCardAddView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public CreditCardAddView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
        #endregion
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			
            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}

