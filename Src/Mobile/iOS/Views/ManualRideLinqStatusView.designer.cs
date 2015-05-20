// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("ManualRideLinqStatusView")]
	partial class ManualRideLinqStatusView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnUnpair { get; set; }

		[Outlet]
		UIKit.UILabel lblDriverId { get; set; }

		[Outlet]
		UIKit.UILabel lblDriverIdText { get; set; }

		[Outlet]
		UIKit.UILabel lblPairingCode { get; set; }

		[Outlet]
		UIKit.UILabel lblPairingCodeText { get; set; }

		[Outlet]
		UIKit.UILabel lblVehicule { get; set; }

		[Outlet]
		UIKit.UILabel lblVehiculeText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnTip != null) {
				btnTip.Dispose ();
				btnTip = null;
			}

			if (btnUnpair != null) {
				btnUnpair.Dispose ();
				btnUnpair = null;
			}

			if (lblDriverId != null) {
				lblDriverId.Dispose ();
				lblDriverId = null;
			}

			if (lblDriverIdText != null) {
				lblDriverIdText.Dispose ();
				lblDriverIdText = null;
			}

			if (lblPairingCode != null) {
				lblPairingCode.Dispose ();
				lblPairingCode = null;
			}

			if (lblPairingCodeText != null) {
				lblPairingCodeText.Dispose ();
				lblPairingCodeText = null;
			}

			if (lblVehicule != null) {
				lblVehicule.Dispose ();
				lblVehicule = null;
			}

			if (lblVehiculeText != null) {
				lblVehiculeText.Dispose ();
				lblVehiculeText = null;
			}
		}
	}
}
