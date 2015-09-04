// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	[Register ("OrderAirportView")]
	partial class OrderAirportView
	{
		[Outlet]
		UIKit.NSLayoutConstraint contraintHeight { get; set; }

		[Outlet]
		UIKit.UILabel lblAirlines { get; set; }

		[Outlet]
		UIKit.UILabel lblAirport { get; set; }

		[Outlet]
		UIKit.UILabel lblFlightNum { get; set; }

		[Outlet]
		UIKit.UILabel lblPickupDate { get; set; }

		[Outlet]
		UIKit.UILabel lblPUPoints { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtAirlines { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtFlightNum { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPickupDate { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtPUPoints { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblAirlines != null) {
				lblAirlines.Dispose ();
				lblAirlines = null;
			}

			if (contraintHeight != null) {
				contraintHeight.Dispose ();
				contraintHeight = null;
			}

			if (lblAirport != null) {
				lblAirport.Dispose ();
				lblAirport = null;
			}

			if (lblFlightNum != null) {
				lblFlightNum.Dispose ();
				lblFlightNum = null;
			}

			if (lblPickupDate != null) {
				lblPickupDate.Dispose ();
				lblPickupDate = null;
			}

			if (lblPUPoints != null) {
				lblPUPoints.Dispose ();
				lblPUPoints = null;
			}

			if (txtAirlines != null) {
				txtAirlines.Dispose ();
				txtAirlines = null;
			}

			if (txtFlightNum != null) {
				txtFlightNum.Dispose ();
				txtFlightNum = null;
			}

			if (txtPickupDate != null) {
				txtPickupDate.Dispose ();
				txtPickupDate = null;
			}

			if (txtPUPoints != null) {
				txtPUPoints.Dispose ();
				txtPUPoints = null;
			}
		}
	}
}
