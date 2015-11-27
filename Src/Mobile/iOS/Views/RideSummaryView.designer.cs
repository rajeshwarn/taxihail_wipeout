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
	[Register ("RideSummaryView")]
	partial class RideSummaryView
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
		UIKit.NSLayoutConstraint constraintRatingTableHeight { get; set; }

		[Outlet]
		UIKit.UILabel gratuityLabel { get; set; }

		[Outlet]
		UIKit.UIView gratuityView { get; set; }

		[Outlet]
		UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		UIKit.UITableView tableRatingList { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
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

			if (constraintRatingTableHeight != null) {
				constraintRatingTableHeight.Dispose ();
				constraintRatingTableHeight = null;
			}

			if (gratuityLabel != null) {
				gratuityLabel.Dispose ();
				gratuityLabel = null;
			}

			if (lblSubTitle != null) {
				lblSubTitle.Dispose ();
				lblSubTitle = null;
			}

			if (tableRatingList != null) {
				tableRatingList.Dispose ();
				tableRatingList = null;
			}

			if (gratuityView != null) {
				gratuityView.Dispose ();
				gratuityView = null;
			}
		}
	}
}
