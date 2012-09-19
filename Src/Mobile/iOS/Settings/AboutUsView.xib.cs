
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class AboutUsView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public AboutUsView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public AboutUsView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public AboutUsView () : base("AboutUsView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
            var url = TinyIoCContainer.Current.Resolve<IAppSettings>().SiteUrl;
            webView.LoadRequest( NSUrlRequest.FromUrl( new NSUrl( url ) ) ); // )  );//  NSUrl.FromFilename( Resources.AboutUsUrl  ) ) );
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

