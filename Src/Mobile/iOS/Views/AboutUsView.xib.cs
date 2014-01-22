using MonoTouch.Foundation;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class AboutUsView : MvxViewController
	{   
		public AboutUsView () 
			: base("AboutUsView", null)
		{
		}

		public new AboutUsViewModel ViewModel
		{
			get
			{
				return (AboutUsViewModel)DataContext;
			}
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
			NavigationItem.Title = Localize.GetValue ("View_AboutUs");
		}
	}
}

