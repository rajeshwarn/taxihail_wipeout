// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views.Cmt
{
	[Register ("CmtHomeView")]
	partial class CmtHomeView
	{
		[Outlet]
		MonoTouch.UIKit.UINavigationBar navBar { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView homeView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (navBar != null) {
				navBar.Dispose ();
				navBar = null;
			}

			if (homeView != null) {
				homeView.Dispose ();
				homeView = null;
			}
		}
	}
}
