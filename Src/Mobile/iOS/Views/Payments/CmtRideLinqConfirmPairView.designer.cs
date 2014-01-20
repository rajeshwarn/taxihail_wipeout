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
		apcurium.MK.Booking.Mobile.Client.Controls.GradientButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.GradientButton btnChangePaymentSettings { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.GradientButton btnConfirm { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblCardNumberValue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblCarNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblCarNumberValue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.FormLabel lblTipValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnChangePaymentSettings != null) {
				btnChangePaymentSettings.Dispose ();
				btnChangePaymentSettings = null;
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

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}
		}
	}
}
