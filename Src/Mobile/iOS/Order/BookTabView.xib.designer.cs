// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BookTabView")]
	partial class BookTabView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressBar pickView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressBar destView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton bookBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView pickDetails { get; set; }

		[Outlet]
		MonoTouch.MapKit.MKMapView mapView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel infoLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton datetimeBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (pickView != null) {
				pickView.Dispose ();
				pickView = null;
			}

			if (destView != null) {
				destView.Dispose ();
				destView = null;
			}

			if (bookBtn != null) {
				bookBtn.Dispose ();
				bookBtn = null;
			}

			if (pickDetails != null) {
				pickDetails.Dispose ();
				pickDetails = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}

			if (infoLabel != null) {
				infoLabel.Dispose ();
				infoLabel = null;
			}

			if (datetimeBtn != null) {
				datetimeBtn.Dispose ();
				datetimeBtn = null;
			}
		}
	}
}
