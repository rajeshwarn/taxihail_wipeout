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
	[Register ("LoginView")]
	partial class LoginView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnFbLogin { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnForgotPassword { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnServer { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnSignIn { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnSignUp { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnSupport { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnTwLogin { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtEmail { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnFbLogin != null) {
				btnFbLogin.Dispose ();
				btnFbLogin = null;
			}

			if (btnForgotPassword != null) {
				btnForgotPassword.Dispose ();
				btnForgotPassword = null;
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

			if (txtEmail != null) {
				txtEmail.Dispose ();
				txtEmail = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (btnSupport != null) {
				btnSupport.Dispose ();
				btnSupport = null;
			}
		}
	}
}
