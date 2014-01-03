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
	[Register ("LoginView")]
	partial class LoginView
	{
		[Outlet]
		MonoTouch.UIKit.UITextField txtEmail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }

		[Outlet]
		GradientButton btnSignIn { get; set; }

		[Outlet]
		UnderlinedLabel linkForgotPassword { get; set; }

		[Outlet]
		GradientButton btnSignUp { get; set; }

		[Outlet]
		GradientButton btnFbLogin { get; set; }

		[Outlet]
		GradientButton btnTwLogin { get; set; }

		[Outlet]
		GradientButton btnServer { get; set; }
		
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
