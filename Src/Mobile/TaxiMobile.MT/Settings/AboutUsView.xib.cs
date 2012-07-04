using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TaxiMobile.Localization;

namespace TaxiMobile.Settings
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
			
			webView.LoadRequest( NSUrlRequest.FromUrl(  NSUrl.FromFilename( Resources.AboutUsUrl  ) ) );
		}
		
		#endregion
	}
}

