// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("StatusView")]
	partial class StatusView
	{
		[Outlet]
		MonoTouch.UIKit.UIView bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnNewRide { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnPay { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCall { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnChangeBooking { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.TouchMap mapStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnRefresh { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStatus { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btnNewRide != null) {
				btnNewRide.Dispose ();
				btnNewRide = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnPay != null) {
				btnPay.Dispose ();
				btnPay = null;
			}

			if (btnCall != null) {
				btnCall.Dispose ();
				btnCall = null;
			}

			if (btnChangeBooking != null) {
				btnChangeBooking.Dispose ();
				btnChangeBooking = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (mapStatus != null) {
				mapStatus.Dispose ();
				mapStatus = null;
			}

			if (btnRefresh != null) {
				btnRefresh.Dispose ();
				btnRefresh = null;
			}

			if (lblStatus != null) {
				lblStatus.Dispose ();
				lblStatus = null;
			}
		}
	}
}
