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
	[Register ("CmtRideLinqConfirmPairView")]
	partial class CmtRideLinqConfirmPairView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnChangePaymentSettings { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCardNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCardNumberValue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCarNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCarNumberValue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTipValue { get; set; }
		
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
		}
	}
}
