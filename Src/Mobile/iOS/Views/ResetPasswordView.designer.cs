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
	[Register ("ResetPasswordView")]
	partial class ResetPasswordView
	{
		[Outlet]
		FormLabel lblEmail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEmail { get; set; }

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

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}
		}
	}
}
