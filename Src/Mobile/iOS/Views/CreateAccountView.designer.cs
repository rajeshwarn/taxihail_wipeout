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
	[Register ("CreateAccountView")]
	partial class CreateAccountView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCreate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnViewTerms { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatCheckBox checkBoxAcknowledged { get; set; }

		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintTableViewHeight { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView tableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnCreate != null) {
				btnCreate.Dispose ();
				btnCreate = null;
			}

			if (constraintTableViewHeight != null) {
				constraintTableViewHeight.Dispose ();
				constraintTableViewHeight = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (checkBoxAcknowledged != null) {
				checkBoxAcknowledged.Dispose ();
				checkBoxAcknowledged = null;
			}

			if (btnViewTerms != null) {
				btnViewTerms.Dispose ();
				btnViewTerms = null;
			}
		}
	}
}
