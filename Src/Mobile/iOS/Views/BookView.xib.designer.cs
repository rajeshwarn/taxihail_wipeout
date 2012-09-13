// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BookView")]
	partial class BookView
	{
		[Outlet]
		MonoTouch.UIKit.UIView view { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel titleLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView menuListView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView logoImageView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel versionLabel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressButton pickupActivationButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressButton dropoffActivationButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressButton pickupButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.AddressButton dropoffButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView headerBackgroundView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton bookLaterButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton refreshCurrentLocationButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton bookBtn { get; set; }

		[Outlet]
		MonoTouch.MapKit.MKMapView mapView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel infoLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (view != null) {
				view.Dispose ();
				view = null;
			}

			if (titleLabel != null) {
				titleLabel.Dispose ();
				titleLabel = null;
			}

			if (menuListView != null) {
				menuListView.Dispose ();
				menuListView = null;
			}

			if (logoImageView != null) {
				logoImageView.Dispose ();
				logoImageView = null;
			}

			if (versionLabel != null) {
				versionLabel.Dispose ();
				versionLabel = null;
			}

			if (pickupActivationButton != null) {
				pickupActivationButton.Dispose ();
				pickupActivationButton = null;
			}

			if (dropoffActivationButton != null) {
				dropoffActivationButton.Dispose ();
				dropoffActivationButton = null;
			}

			if (pickupButton != null) {
				pickupButton.Dispose ();
				pickupButton = null;
			}

			if (dropoffButton != null) {
				dropoffButton.Dispose ();
				dropoffButton = null;
			}

			if (headerBackgroundView != null) {
				headerBackgroundView.Dispose ();
				headerBackgroundView = null;
			}

			if (bookLaterButton != null) {
				bookLaterButton.Dispose ();
				bookLaterButton = null;
			}

			if (refreshCurrentLocationButton != null) {
				refreshCurrentLocationButton.Dispose ();
				refreshCurrentLocationButton = null;
			}

			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (bookBtn != null) {
				bookBtn.Dispose ();
				bookBtn = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}

			if (infoLabel != null) {
				infoLabel.Dispose ();
				infoLabel = null;
			}
		}
	}
}
