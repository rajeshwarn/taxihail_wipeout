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
	[Register ("BookingBarInTripNormalBooking")]
	partial class BookingBarInTripNormalBooking
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonCall { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonEditTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonUnpair { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.HideableView viewCancelEditTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.HideableView viewUnpairEditTip { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (buttonCall != null) {
				buttonCall.Dispose ();
				buttonCall = null;
			}

			if (buttonCancel != null) {
				buttonCancel.Dispose ();
				buttonCancel = null;
			}

			if (buttonEditTip != null) {
				buttonEditTip.Dispose ();
				buttonEditTip = null;
			}

			if (buttonUnpair != null) {
				buttonUnpair.Dispose ();
				buttonUnpair = null;
			}

			if (viewCancelEditTip != null) {
				viewCancelEditTip.Dispose ();
				viewCancelEditTip = null;
			}

			if (viewUnpairEditTip != null) {
				viewUnpairEditTip.Dispose ();
				viewUnpairEditTip = null;
			}
		}
	}
}
