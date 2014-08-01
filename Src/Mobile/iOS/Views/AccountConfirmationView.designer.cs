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
	[Register ("AccountConfirmationView")]
	partial class AccountConfirmationView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgViewLogo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCode { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (txtCode != null) {
				txtCode.Dispose ();
				txtCode = null;
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

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}
		}
	}
}
