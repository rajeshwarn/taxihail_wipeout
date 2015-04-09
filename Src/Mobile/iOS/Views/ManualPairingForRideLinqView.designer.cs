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
	[Register ("ManualPairingForRideLinqView")]
	partial class ManualPairingForRideLinqView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnPair { get; set; }

		[Outlet]
		UIKit.UILabel lblInstructions { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField PairingCode1 { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField PairingCode2 { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnPair != null) {
				btnPair.Dispose ();
				btnPair = null;
			}

			if (lblInstructions != null) {
				lblInstructions.Dispose ();
				lblInstructions = null;
			}

			if (PairingCode1 != null) {
				PairingCode1.Dispose ();
				PairingCode1 = null;
			}

			if (PairingCode2 != null) {
				PairingCode2.Dispose ();
				PairingCode2 = null;
			}
		}
	}
}
