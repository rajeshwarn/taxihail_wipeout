using Foundation;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Style;
using UIKit;

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

            NavigationItem.Title = Localize.GetValue ("View_AboutUs");
			NavigationController.NavigationBar.Hidden = false;
            NavigationController.NavigationBar.BarStyle = Theme.IsLightContent
                ? UIBarStyle.Black
                : UIBarStyle.Default;
		}
	}
}

