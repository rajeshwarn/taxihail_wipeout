// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
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
		MonoTouch.UIKit.UIView topVisibleStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView topSlidingStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblConfirmation { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDriver { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblLicence { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTaxiType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblMake { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblModel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblColor { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnNewRide { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCall { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnChangeBooking { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.StatusBar statusBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.TouchMap mapStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnRefresh { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (topVisibleStatus != null) {
				topVisibleStatus.Dispose ();
				topVisibleStatus = null;
			}

			if (topSlidingStatus != null) {
				topSlidingStatus.Dispose ();
				topSlidingStatus = null;
			}

			if (lblConfirmation != null) {
				lblConfirmation.Dispose ();
				lblConfirmation = null;
			}

			if (lblStatus != null) {
				lblStatus.Dispose ();
				lblStatus = null;
			}

			if (lblDriver != null) {
				lblDriver.Dispose ();
				lblDriver = null;
			}

			if (lblLicence != null) {
				lblLicence.Dispose ();
				lblLicence = null;
			}

			if (lblTaxiType != null) {
				lblTaxiType.Dispose ();
				lblTaxiType = null;
			}

			if (lblMake != null) {
				lblMake.Dispose ();
				lblMake = null;
			}

			if (lblModel != null) {
				lblModel.Dispose ();
				lblModel = null;
			}

			if (lblColor != null) {
				lblColor.Dispose ();
				lblColor = null;
			}

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

			if (btnCall != null) {
				btnCall.Dispose ();
				btnCall = null;
			}

			if (btnChangeBooking != null) {
				btnChangeBooking.Dispose ();
				btnChangeBooking = null;
			}

			if (statusBar != null) {
				statusBar.Dispose ();
				statusBar = null;
			}

			if (mapStatus != null) {
				mapStatus.Dispose ();
				mapStatus = null;
			}

			if (btnRefresh != null) {
				btnRefresh.Dispose ();
				btnRefresh = null;
			}
		}
	}
}
