using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class PayPalView : BaseViewController<PayPalViewModel>
    {
        public PayPalView(MvxShowViewModelRequest request) 
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
            ViewModel.Load();

			// Hide navigation bar, full screen height is required for displaying paypal page
			this.NavigationController.NavigationBarHidden = true;

            webView.ShouldStartLoad = LoadHook;
			var request = new NSUrlRequest (new NSUrl (ViewModel.Url));
            
			webView.LoadRequest(request);
			webView.LoadFinished += (s,e) => {
				ViewModel.WebViewLoadFinished();
			}; 
        }

        bool LoadHook (UIWebView sender, NSUrlRequest request, UIWebViewNavigationType navType)
        {
			if(request.Url.Scheme.StartsWith("taxihail"))
			{
				if (request.Url.Host == "success") {
					ViewModel.Finish.Execute (true);
				} else {
					ViewModel.Finish.Execute (false);
				}
				return false;
			}
			return true;
        }
    }
}

