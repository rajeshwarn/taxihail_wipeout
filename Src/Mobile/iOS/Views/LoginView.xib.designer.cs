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
	[Register ("LoginView")]
	partial class LoginView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnFbLogin { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnForgotPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnServer { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSignIn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSignUp { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnTwLogin { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEmail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnFbLogin != null) {
				btnFbLogin.Dispose ();
				btnFbLogin = null;
			}

			if (btnServer != null) {
				btnServer.Dispose ();
				btnServer = null;
			}

			if (btnSignIn != null) {
				btnSignIn.Dispose ();
				btnSignIn = null;
			}

			if (btnSignUp != null) {
				btnSignUp.Dispose ();
				btnSignUp = null;
			}

			if (btnTwLogin != null) {
				btnTwLogin.Dispose ();
				btnTwLogin = null;
			}

			if (btnForgotPassword != null) {
				btnForgotPassword.Dispose ();
				btnForgotPassword = null;
			}

			if (txtEmail != null) {
				txtEmail.Dispose ();
				txtEmail = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}
		}
	}
}
