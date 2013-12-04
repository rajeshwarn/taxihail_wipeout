
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class AboutUsView : MvxBindingTouchViewController<AboutUsViewModel> 
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

        public AboutUsView () 
            : base(new MvxShowViewModelRequest<AboutUsViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public AboutUsView (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public AboutUsView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			            
            webView.LoadRequest( NSUrlRequest.FromUrl( new NSUrl( ViewModel.Uri ) ) ); 
            webView.ScalesPageToFit = true;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
		}
		
		#endregion
	}
}

