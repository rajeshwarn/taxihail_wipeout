// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	[Register ("OrderEditView")]
	partial class OrderEditView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblApartment { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblEntryCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPassengers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVehicleType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtApartment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPassengers { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPhone { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtVehicleType { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (txtPhone != null) {
				txtPhone.Dispose ();
				txtPhone = null;
			}

			if (lblPassengers != null) {
				lblPassengers.Dispose ();
				lblPassengers = null;
			}

			if (txtPassengers != null) {
				txtPassengers.Dispose ();
				txtPassengers = null;
			}

			if (lblApartment != null) {
				lblApartment.Dispose ();
				lblApartment = null;
			}

			if (txtApartment != null) {
				txtApartment.Dispose ();
				txtApartment = null;
			}

			if (lblEntryCode != null) {
				lblEntryCode.Dispose ();
				lblEntryCode = null;
			}

			if (txtEntryCode != null) {
				txtEntryCode.Dispose ();
				txtEntryCode = null;
			}

			if (lblVehicleType != null) {
				lblVehicleType.Dispose ();
				lblVehicleType = null;
			}

			if (txtVehicleType != null) {
				txtVehicleType.Dispose ();
				txtVehicleType = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (txtChargeType != null) {
				txtChargeType.Dispose ();
				txtChargeType = null;
			}
		}
	}
}
