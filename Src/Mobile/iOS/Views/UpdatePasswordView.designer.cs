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
	[Register ("UpdatePasswordView")]
	partial class UpdatePasswordView
	{
		[Outlet]
		UIKit.UILabel lblConfirmation { get; set; }

		[Outlet]
		UIKit.UILabel lblCurrentPassword { get; set; }

		[Outlet]
		UIKit.UILabel lblNewPassword { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtConfirmation { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCurrentPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtNewPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (lblConfirmation != null) {
				lblConfirmation.Dispose ();
				lblConfirmation = null;
			}

			if (lblCurrentPassword != null) {
				lblCurrentPassword.Dispose ();
				lblCurrentPassword = null;
			}

			if (lblNewPassword != null) {
				lblNewPassword.Dispose ();
				lblNewPassword = null;
			}

			if (txtConfirmation != null) {
				txtConfirmation.Dispose ();
				txtConfirmation = null;
			}

			if (txtCurrentPassword != null) {
				txtCurrentPassword.Dispose ();
				txtCurrentPassword = null;
			}

			if (txtNewPassword != null) {
				txtNewPassword.Dispose ();
				txtNewPassword = null;
			}
		}
	}
}
