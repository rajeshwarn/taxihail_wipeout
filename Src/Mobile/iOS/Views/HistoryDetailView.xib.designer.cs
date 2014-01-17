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
	[Register ("HistoryDetailView")]
	partial class HistoryDetailView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnDelete { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnRebook { get; set; }

		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintDestinationLabelToAptRingCodeLabelHeight { get; set; }

		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintDestinationTextToAptRingCodeTextHeight { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblAptRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblOrder { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPickup { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPickupDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRequested { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtAptRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtOrder { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtPickup { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtPickupDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtRequested { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtStatus { get; set; }
		
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

			if (constraintDestinationLabelToAptRingCodeLabelHeight != null) {
				constraintDestinationLabelToAptRingCodeLabelHeight.Dispose ();
				constraintDestinationLabelToAptRingCodeLabelHeight = null;
			}

			if (constraintDestinationTextToAptRingCodeTextHeight != null) {
				constraintDestinationTextToAptRingCodeTextHeight.Dispose ();
				constraintDestinationTextToAptRingCodeTextHeight = null;
			}

			if (lblAptRingCode != null) {
				lblAptRingCode.Dispose ();
				lblAptRingCode = null;
			}

			if (lblDestination != null) {
				lblDestination.Dispose ();
				lblDestination = null;
			}

			if (lblOrder != null) {
				lblOrder.Dispose ();
				lblOrder = null;
			}

			if (lblPickup != null) {
				lblPickup.Dispose ();
				lblPickup = null;
			}

			if (lblPickupDate != null) {
				lblPickupDate.Dispose ();
				lblPickupDate = null;
			}

			if (lblRequested != null) {
				lblRequested.Dispose ();
				lblRequested = null;
			}

			if (lblStatus != null) {
				lblStatus.Dispose ();
				lblStatus = null;
			}

			if (txtAptRingCode != null) {
				txtAptRingCode.Dispose ();
				txtAptRingCode = null;
			}

			if (txtDestination != null) {
				txtDestination.Dispose ();
				txtDestination = null;
			}

			if (txtOrder != null) {
				txtOrder.Dispose ();
				txtOrder = null;
			}

			if (txtPickup != null) {
				txtPickup.Dispose ();
				txtPickup = null;
			}

			if (txtPickupDate != null) {
				txtPickupDate.Dispose ();
				txtPickupDate = null;
			}

			if (txtRequested != null) {
				txtRequested.Dispose ();
				txtRequested = null;
			}

			if (txtStatus != null) {
				txtStatus.Dispose ();
				txtStatus = null;
			}
		}
	}
}
