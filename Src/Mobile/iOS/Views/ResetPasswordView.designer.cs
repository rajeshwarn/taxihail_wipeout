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
	[Register ("ResetPasswordView")]
	partial class ResetPasswordView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnReset { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgViewLogo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtEmail { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnReset != null) {
				btnReset.Dispose ();
				btnReset = null;
			}

			if (imgViewLogo != null) {
				imgViewLogo.Dispose ();
				imgViewLogo = null;
			}

			if (lblSubTitle != null) {
				lblSubTitle.Dispose ();
				lblSubTitle = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (txtEmail != null) {
				txtEmail.Dispose ();
				txtEmail = null;
			}
		}
	}
}
