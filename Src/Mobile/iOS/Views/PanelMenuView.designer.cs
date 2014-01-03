// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("PanelMenuView")]
	partial class PanelMenuView
	{
		[Outlet]
		PanelView panelView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView menuView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView menuListView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView logoImageView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel versionLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (panelView != null) {
				panelView.Dispose ();
				panelView = null;
			}

			if (menuView != null) {
				menuView.Dispose ();
				menuView = null;
			}

			if (menuListView != null) {
				menuListView.Dispose ();
				menuListView = null;
			}

			if (logoImageView != null) {
				logoImageView.Dispose ();
				logoImageView = null;
			}

			if (versionLabel != null) {
				versionLabel.Dispose ();
				versionLabel = null;
			}
		}
	}
}
