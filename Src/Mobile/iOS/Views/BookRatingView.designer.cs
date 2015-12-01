// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookRatingView")]
	partial class BookRatingView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnGratuity0 { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnGratuity1 { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnGratuity2 { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnGratuity3 { get; set; }

		[Outlet]
		UIKit.UILabel gratuityLabel { get; set; }

		[Outlet]
		UIKit.UIView gratuityView { get; set; }

		[Outlet]
		UIKit.UITableView ratingTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (gratuityLabel != null) {
				gratuityLabel.Dispose ();
				gratuityLabel = null;
			}

			if (gratuityView != null) {
				gratuityView.Dispose ();
				gratuityView = null;
			}

			if (ratingTableView != null) {
				ratingTableView.Dispose ();
				ratingTableView = null;
			}

			if (btnGratuity0 != null) {
				btnGratuity0.Dispose ();
				btnGratuity0 = null;
			}

			if (btnGratuity1 != null) {
				btnGratuity1.Dispose ();
				btnGratuity1 = null;
			}

			if (btnGratuity2 != null) {
				btnGratuity2.Dispose ();
				btnGratuity2 = null;
			}

			if (btnGratuity3 != null) {
				btnGratuity3.Dispose ();
				btnGratuity3 = null;
			}
		}
	}
}
