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
	[Register ("LocationDetailView")]
	partial class LocationDetailView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnDelete { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnRebook { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblApartment { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRingCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtAddress { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtAptNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtRingCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnDelete != null) {
				btnDelete.Dispose ();
				btnDelete = null;
			}

			if (btnRebook != null) {
				btnRebook.Dispose ();
				btnRebook = null;
			}

			if (lblAddress != null) {
				lblAddress.Dispose ();
				lblAddress = null;
			}

			if (lblApartment != null) {
				lblApartment.Dispose ();
				lblApartment = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (lblRingCode != null) {
				lblRingCode.Dispose ();
				lblRingCode = null;
			}

			if (txtAddress != null) {
				txtAddress.Dispose ();
				txtAddress = null;
			}

			if (txtAptNumber != null) {
				txtAptNumber.Dispose ();
				txtAptNumber = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (txtRingCode != null) {
				txtRingCode.Dispose ();
				txtRingCode = null;
			}
		}
	}
}
