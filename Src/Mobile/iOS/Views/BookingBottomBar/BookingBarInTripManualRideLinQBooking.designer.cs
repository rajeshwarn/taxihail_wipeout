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
	[Register ("BookingBarInTripManualRideLinQBooking")]
	partial class BookingBarInTripManualRideLinQBooking
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonCall { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonTipChange { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonUnpair { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.HindableView viewUnpairTipChange { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (buttonCall != null) {
				buttonCall.Dispose ();
				buttonCall = null;
			}

			if (buttonTipChange != null) {
				buttonTipChange.Dispose ();
				buttonTipChange = null;
			}

			if (buttonUnpair != null) {
				buttonUnpair.Dispose ();
				buttonUnpair = null;
			}

			if (viewUnpairTipChange != null) {
				viewUnpairTipChange.Dispose ();
				viewUnpairTipChange = null;
			}
		}
	}
}
