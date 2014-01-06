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
		apcurium.MK.Booking.Mobile.Client.GradientButton btnChangePaymentSettings { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCardNumberValue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCarNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCarNumberValue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTipValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblCarNumber != null) {
				lblCarNumber.Dispose ();
				lblCarNumber = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (lblTip != null) {
				lblTip.Dispose ();
				lblTip = null;
			}

			if (lblCarNumberValue != null) {
				lblCarNumberValue.Dispose ();
				lblCarNumberValue = null;
			}

			if (lblCardNumberValue != null) {
				lblCardNumberValue.Dispose ();
				lblCardNumberValue = null;
			}

			if (lblTipValue != null) {
				lblTipValue.Dispose ();
				lblTipValue = null;
			}

			if (btnChangePaymentSettings != null) {
				btnChangePaymentSettings.Dispose ();
				btnChangePaymentSettings = null;
			}
		}
	}
}
