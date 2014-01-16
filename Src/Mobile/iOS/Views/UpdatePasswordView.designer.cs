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
	[Register ("UpdatePasswordView")]
	partial class UpdatePasswordView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblConfirmation { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCurrentPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNewPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtConfirmation { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCurrentPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtNewPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (lblCurrentPassword != null) {
				lblCurrentPassword.Dispose ();
				lblCurrentPassword = null;
			}

			if (lblNewPassword != null) {
				lblNewPassword.Dispose ();
				lblNewPassword = null;
			}

			if (lblConfirmation != null) {
				lblConfirmation.Dispose ();
				lblConfirmation = null;
			}

			if (txtCurrentPassword != null) {
				txtCurrentPassword.Dispose ();
				txtCurrentPassword = null;
			}

			if (txtNewPassword != null) {
				txtNewPassword.Dispose ();
				txtNewPassword = null;
			}

			if (txtConfirmation != null) {
				txtConfirmation.Dispose ();
				txtConfirmation = null;
			}
		}
	}
}
