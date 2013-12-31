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
	[Register ("CreateAccountView")]
	partial class CreateAccountView
	{
		[Outlet]
		FormLabel lblEmail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEmail { get; set; }

		[Outlet]
		FormLabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtName { get; set; }

		[Outlet]
		FormLabel lblPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPhone { get; set; }

		[Outlet]
		FormLabel lblPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }

		[Outlet]
		FormLabel lblConfirmPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtConfirmPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblEmail != null) {
				lblEmail.Dispose ();
				lblEmail = null;
			}

			if (txtEmail != null) {
				txtEmail.Dispose ();
				txtEmail = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (txtPhone != null) {
				txtPhone.Dispose ();
				txtPhone = null;
			}

			if (lblPassword != null) {
				lblPassword.Dispose ();
				lblPassword = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (lblConfirmPassword != null) {
				lblConfirmPassword.Dispose ();
				lblConfirmPassword = null;
			}

			if (txtConfirmPassword != null) {
				txtConfirmPassword.Dispose ();
				txtConfirmPassword = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}
		}
	}
}
