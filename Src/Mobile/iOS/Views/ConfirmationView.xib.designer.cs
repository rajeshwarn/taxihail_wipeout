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
		apcurium.MK.Booking.Mobile.Client.FormLabel lblApartment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblNoteDriver { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtApartment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerChargeType { get; set; }

		[Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerVehicleType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVehiculeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.TextView txtNotes { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblApartment != null) {
				lblApartment.Dispose ();
				lblApartment = null;
			}

			if (lblNoteDriver != null) {
				lblNoteDriver.Dispose ();
				lblNoteDriver = null;
			}

			if (lblEntryCode != null) {
				lblEntryCode.Dispose ();
				lblEntryCode = null;
			}

			if (txtEntryCode != null) {
				txtEntryCode.Dispose ();
				txtEntryCode = null;
			}

			if (txtApartment != null) {
				txtApartment.Dispose ();
				txtApartment = null;
			}

			if (pickerChargeType != null) {
				pickerChargeType.Dispose ();
				pickerChargeType = null;
			}

			if (pickerVehicleType != null) {
				pickerVehicleType.Dispose ();
				pickerVehicleType = null;
			}

			if (lblVehiculeType != null) {
				lblVehiculeType.Dispose ();
				lblVehiculeType = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (txtNotes != null) {
				txtNotes.Dispose ();
				txtNotes = null;
			}
		}
	}
}
