// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("SettingsTabView")]
	partial class SettingsTabView
	{
		[Outlet]
		MonoTouch.UIKit.UIView contentView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblServerName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblServerVersion { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnChangeSettings { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSignOut { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblLoginStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVersion { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnError { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCall { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgCreatedBy { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnTechSupport { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnAbout { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (contentView != null) {
				contentView.Dispose ();
				contentView = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (lblServerName != null) {
				lblServerName.Dispose ();
				lblServerName = null;
			}

			if (lblServerVersion != null) {
				lblServerVersion.Dispose ();
				lblServerVersion = null;
			}

			if (btnChangeSettings != null) {
				btnChangeSettings.Dispose ();
				btnChangeSettings = null;
			}

			if (btnSignOut != null) {
				btnSignOut.Dispose ();
				btnSignOut = null;
			}

			if (lblLoginStatus != null) {
				lblLoginStatus.Dispose ();
				lblLoginStatus = null;
			}

			if (lblVersion != null) {
				lblVersion.Dispose ();
				lblVersion = null;
			}

			if (btnError != null) {
				btnError.Dispose ();
				btnError = null;
			}

			if (btnCall != null) {
				btnCall.Dispose ();
				btnCall = null;
			}

			if (imgCreatedBy != null) {
				imgCreatedBy.Dispose ();
				imgCreatedBy = null;
			}

			if (btnTechSupport != null) {
				btnTechSupport.Dispose ();
				btnTechSupport = null;
			}

			if (btnAbout != null) {
				btnAbout.Dispose ();
				btnAbout = null;
			}
		}
	}
}
