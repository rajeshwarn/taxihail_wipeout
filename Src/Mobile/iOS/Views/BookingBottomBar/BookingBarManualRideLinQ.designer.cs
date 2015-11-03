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
	[Register ("BookingBarManualRideLinQ")]
	partial class BookingBarManualRideLinQ
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonBooking { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarButton buttonEstimate { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonManualRideLinQBooking { get; set; }

		[Outlet]
		UIKit.UIImageView imagePromotional { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.HindableView viewEstimate { get; set; }

		[Outlet]
		UIKit.UIView viewManualRideLinQBooking { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (buttonBooking != null) {
				buttonBooking.Dispose ();
				buttonBooking = null;
			}

			if (buttonEstimate != null) {
				buttonEstimate.Dispose ();
				buttonEstimate = null;
			}

			if (buttonManualRideLinQBooking != null) {
				buttonManualRideLinQBooking.Dispose ();
				buttonManualRideLinQBooking = null;
			}

			if (viewEstimate != null) {
				viewEstimate.Dispose ();
				viewEstimate = null;
			}

			if (viewManualRideLinQBooking != null) {
				viewManualRideLinQBooking.Dispose ();
				viewManualRideLinQBooking = null;
			}

			if (imagePromotional != null) {
				imagePromotional.Dispose ();
				imagePromotional = null;
			}
		}
	}
}
