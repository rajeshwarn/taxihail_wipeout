// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("PanelMenuView")]
	partial class PanelMenuView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblVersion { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView logoImageView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView menuContainer { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView menuListView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (logoImageView != null) {
				logoImageView.Dispose ();
				logoImageView = null;
			}

			if (menuContainer != null) {
				menuContainer.Dispose ();
				menuContainer = null;
			}

			if (menuListView != null) {
				menuListView.Dispose ();
				menuListView = null;
			}

			if (lblVersion != null) {
				lblVersion.Dispose ();
				lblVersion = null;
			}
		}
	}
}
