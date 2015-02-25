// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("PanelMenuView")]
	partial class PanelMenuView
	{
		[Outlet]
		UIKit.UIImageView imgLogoApcurium { get; set; }

		[Outlet]
		UIKit.UIImageView imgLogoMobileKnowledge { get; set; }

		[Outlet]
		UIKit.UILabel lblServerVersion { get; set; }

		[Outlet]
		UIKit.UILabel lblVersion { get; set; }

		[Outlet]
		UIKit.UIImageView logoImageView { get; set; }

		[Outlet]
		UIKit.UIView menuContainer { get; set; }

		[Outlet]
		UIKit.UITableView menuListView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgLogoApcurium != null) {
				imgLogoApcurium.Dispose ();
				imgLogoApcurium = null;
			}

			if (imgLogoMobileKnowledge != null) {
				imgLogoMobileKnowledge.Dispose ();
				imgLogoMobileKnowledge = null;
			}

			if (lblVersion != null) {
				lblVersion.Dispose ();
				lblVersion = null;
			}

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

			if (lblServerVersion != null) {
				lblServerVersion.Dispose ();
				lblServerVersion = null;
			}
		}
	}
}
