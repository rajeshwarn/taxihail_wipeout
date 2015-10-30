// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("BookingBarNormalBooking")]
	partial class BookingBarNormalBooking
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarButton buttonReservationBooking { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonBooking { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarButton buttonEstimate { get; set; }

		[Outlet]
		UIKit.UIImageView imagePromotion { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (buttonReservationBooking != null) {
				buttonReservationBooking.Dispose ();
				buttonReservationBooking = null;
			}

			if (buttonBooking != null) {
				buttonBooking.Dispose ();
				buttonBooking = null;
			}

			if (buttonEstimate != null) {
				buttonEstimate.Dispose ();
				buttonEstimate = null;
			}

			if (imagePromotion != null) {
				imagePromotion.Dispose ();
				imagePromotion = null;
			}
		}
	}
}
