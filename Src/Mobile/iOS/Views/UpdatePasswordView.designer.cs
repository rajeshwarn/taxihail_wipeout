// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("UpdatePasswordView")]
	partial class UpdatePasswordView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblCurrentPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNewPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNewPasswordConfirmation { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtCurrentPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtNewPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtNewPasswordConfirmation { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblCurrentPassword != null) {
				lblCurrentPassword.Dispose ();
				lblCurrentPassword = null;
			}

			if (lblNewPassword != null) {
				lblNewPassword.Dispose ();
				lblNewPassword = null;
			}

			if (lblNewPasswordConfirmation != null) {
				lblNewPasswordConfirmation.Dispose ();
				lblNewPasswordConfirmation = null;
			}

			if (txtCurrentPassword != null) {
				txtCurrentPassword.Dispose ();
				txtCurrentPassword = null;
			}

			if (txtNewPassword != null) {
				txtNewPassword.Dispose ();
				txtNewPassword = null;
			}

			if (txtNewPasswordConfirmation != null) {
				txtNewPasswordConfirmation.Dispose ();
				txtNewPasswordConfirmation = null;
			}
		}
	}
}
