// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("HomeView")]
	partial class HomeView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptionsControl ctrlOrderOptions { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView homeView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayView locateMeOverlay { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.OrderMapView mapView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayView test { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPickupAddress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (homeView != null) {
				homeView.Dispose ();
				homeView = null;
			}

			if (locateMeOverlay != null) {
				locateMeOverlay.Dispose ();
				locateMeOverlay = null;
			}

			if (test != null) {
				test.Dispose ();
				test = null;
			}

			if (txtPickupAddress != null) {
				txtPickupAddress.Dispose ();
				txtPickupAddress = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}
			
			if (ctrlOrderOptions != null) {
				ctrlOrderOptions.Dispose ();
				ctrlOrderOptions = null;
			}

		}
	}
}
