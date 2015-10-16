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
	[Register ("AccountConfirmationView")]
	partial class AccountConfirmationView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnResend { get; set; }

		[Outlet]
		UIKit.UIScrollView confirmScrollViewer { get; set; }

		[Outlet]
		UIKit.UIImageView imgViewLogo { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.CountrySelector lblDialCode { get; set; }

		[Outlet]
		UIKit.UILabel lblPhoneNumberTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPhoneNumber { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (btnResend != null) {
				btnResend.Dispose ();
				btnResend = null;
			}

			if (imgViewLogo != null) {
				imgViewLogo.Dispose ();
				imgViewLogo = null;
			}

			if (lblDialCode != null) {
				lblDialCode.Dispose ();
				lblDialCode = null;
			}

			if (lblPhoneNumberTitle != null) {
				lblPhoneNumberTitle.Dispose ();
				lblPhoneNumberTitle = null;
			}

			if (lblSubTitle != null) {
				lblSubTitle.Dispose ();
				lblSubTitle = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (txtCode != null) {
				txtCode.Dispose ();
				txtCode = null;
			}

			if (txtPhoneNumber != null) {
				txtPhoneNumber.Dispose ();
				txtPhoneNumber = null;
			}

			if (confirmScrollViewer != null) {
				confirmScrollViewer.Dispose ();
				confirmScrollViewer = null;
			}
		}
	}
}
