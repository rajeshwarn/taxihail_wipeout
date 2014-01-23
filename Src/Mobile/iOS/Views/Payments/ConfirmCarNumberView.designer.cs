// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("ConfirmCarNumberView")]
	partial class ConfirmCarNumberView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCarNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblConfirmDriverInfo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblConfirmDriverNotice { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblConfirmDriverInfo != null) {
				lblConfirmDriverInfo.Dispose ();
				lblConfirmDriverInfo = null;
			}

			if (lblConfirmDriverNotice != null) {
				lblConfirmDriverNotice.Dispose ();
				lblConfirmDriverNotice = null;
			}

			if (lblCarNumber != null) {
				lblCarNumber.Dispose ();
				lblCarNumber = null;
			}

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}
		}
	}
}
