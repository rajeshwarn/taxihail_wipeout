// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("ConfirmPairView")]
	partial class ConfirmPairView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnChangePaymentSettings { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		UIKit.UILabel lblCardNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblCardNumberValue { get; set; }

		[Outlet]
		UIKit.UILabel lblCarNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblCarNumberValue { get; set; }

		[Outlet]
		UIKit.UILabel lblConfirmPairDetail { get; set; }

		[Outlet]
		UIKit.UILabel lblTip { get; set; }

		[Outlet]
		UIKit.UILabel lblTipValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnChangePaymentSettings != null) {
				btnChangePaymentSettings.Dispose ();
				btnChangePaymentSettings = null;
			}

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (lblCardNumberValue != null) {
				lblCardNumberValue.Dispose ();
				lblCardNumberValue = null;
			}

			if (lblCarNumber != null) {
				lblCarNumber.Dispose ();
				lblCarNumber = null;
			}

			if (lblCarNumberValue != null) {
				lblCarNumberValue.Dispose ();
				lblCarNumberValue = null;
			}

			if (lblTip != null) {
				lblTip.Dispose ();
				lblTip = null;
			}

			if (lblTipValue != null) {
				lblTipValue.Dispose ();
				lblTipValue = null;
			}

			if (lblConfirmPairDetail != null) {
				lblConfirmPairDetail.Dispose ();
				lblConfirmPairDetail = null;
			}
		}
	}
}
