// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("LoginView")]
	partial class LoginView
	{
		[Outlet]
		MonoTouch.UIKit.UITextField txtEmail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnSignIn { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.UnderlinedLabel linkForgotPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnSignUp { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnFbLogin { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnTwLogin { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnServer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtEmail != null) {
				txtEmail.Dispose ();
				txtEmail = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (btnSignIn != null) {
				btnSignIn.Dispose ();
				btnSignIn = null;
			}

			if (linkForgotPassword != null) {
				linkForgotPassword.Dispose ();
				linkForgotPassword = null;
			}

			if (btnSignUp != null) {
				btnSignUp.Dispose ();
				btnSignUp = null;
			}

			if (btnFbLogin != null) {
				btnFbLogin.Dispose ();
				btnFbLogin = null;
			}

			if (btnTwLogin != null) {
				btnTwLogin.Dispose ();
				btnTwLogin = null;
			}

			if (btnServer != null) {
				btnServer.Dispose ();
				btnServer = null;
			}
		}
	}
}
