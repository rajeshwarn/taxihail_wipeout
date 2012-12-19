// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("ConfirmationView")]
	partial class ConfirmationView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerAptEntryBuilding { get; set; }

        [Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerVehicleType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblAptRing { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblVehiculeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPickupDetails { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDateTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblOrigin { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtDateTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtOrigin { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPrice { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtPrice { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (pickerAptEntryBuilding != null) {
				pickerAptEntryBuilding.Dispose ();
				pickerAptEntryBuilding = null;
			}

			if (pickerChargeType != null) {
				pickerChargeType.Dispose ();
				pickerChargeType = null;
			}

			if (pickerVehicleType != null) {
				pickerVehicleType.Dispose ();
				pickerVehicleType = null;
			}

			if (lblAptRing != null) {
				lblAptRing.Dispose ();
				lblAptRing = null;
			}

			if (lblVehiculeType != null) {
				lblVehiculeType.Dispose ();
				lblVehiculeType = null;
			}

			if (lblPickupDetails != null) {
				lblPickupDetails.Dispose ();
				lblPickupDetails = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (lblDateTime != null) {
				lblDateTime.Dispose ();
				lblDateTime = null;
			}

			if (lblDestination != null) {
				lblDestination.Dispose ();
				lblDestination = null;
			}

			if (lblOrigin != null) {
				lblOrigin.Dispose ();
				lblOrigin = null;
			}

			if (txtDateTime != null) {
				txtDateTime.Dispose ();
				txtDateTime = null;
			}

			if (txtDestination != null) {
				txtDestination.Dispose ();
				txtDestination = null;
			}

			if (txtOrigin != null) {
				txtOrigin.Dispose ();
				txtOrigin = null;
			}

			if (lblPrice != null) {
				lblPrice.Dispose ();
				lblPrice = null;
			}

			if (txtPrice != null) {
				txtPrice.Dispose ();
				txtPrice = null;
			}
		}
	}
}
