// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BookView")]
	partial class BookView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton backBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton bookBtn { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton bookLaterButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView bookView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton cancelBtn { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton dropoffActivationButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextProgressButton dropoffButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView headerBackgroundView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel infoLabel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.TouchMap mapView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UINavigationBar navBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton pickupActivationButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextProgressButton pickupButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton refreshCurrentLocationButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (cancelBtn != null) {
				cancelBtn.Dispose ();
				cancelBtn = null;
			}

			if (navBar != null) {
				navBar.Dispose ();
				navBar = null;
			}

			if (bookView != null) {
				bookView.Dispose ();
				bookView = null;
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

			if (backBtn != null) {
				backBtn.Dispose ();
				backBtn = null;
			}
		}
	}
}
