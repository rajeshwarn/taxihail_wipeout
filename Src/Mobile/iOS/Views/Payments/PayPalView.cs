using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class PayPalView : BaseViewController<PayPalViewModel>
    {
        public PayPalView() : base("PayPalView", null)
        {
        }
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Hide navigation bar, full screen height is required for displaying paypal page
            NavigationController.NavigationBarHidden = true;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

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
