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
	[Register ("OrderBookingOptions")]
	partial class OrderBookingOptions
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnLater { get; set; }

		[Outlet]
		UIKit.UIButton btnNow { get; set; }

		[Outlet]
		UIKit.UILabel lblDescription { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblDescription != null) {
				lblDescription.Dispose ();
				lblDescription = null;
			}

			if (btnNow != null) {
				btnNow.Dispose ();
				btnNow = null;
			}

			if (btnLater != null) {
				btnLater.Dispose ();
				btnLater = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}
		}
	}
}
