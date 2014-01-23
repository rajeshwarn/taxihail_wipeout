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
	[Register ("RideSummaryView")]
	partial class RideSummaryView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnPay { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnRateRide { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnReSendConfirmation { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnSendReceipt { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnPay != null) {
				btnPay.Dispose ();
				btnPay = null;
			}

			if (btnRateRide != null) {
				btnRateRide.Dispose ();
				btnRateRide = null;
			}

			if (btnReSendConfirmation != null) {
				btnReSendConfirmation.Dispose ();
				btnReSendConfirmation = null;
			}

			if (btnSendReceipt != null) {
				btnSendReceipt.Dispose ();
				btnSendReceipt = null;
			}

			if (lblSubTitle != null) {
				lblSubTitle.Dispose ();
				lblSubTitle = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}
		}
	}
}
