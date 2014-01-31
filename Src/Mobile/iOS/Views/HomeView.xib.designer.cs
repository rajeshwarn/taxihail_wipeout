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
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarView bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnLocateMe { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnMenu { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptionsControl ctrlOrderOptions { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView homeView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.OrderMapView mapView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btnMenu != null) {
				btnMenu.Dispose ();
				btnMenu = null;
			}

			if (btnLocateMe != null) {
				btnLocateMe.Dispose ();
				btnLocateMe = null;
			}

			if (ctrlOrderOptions != null) {
				ctrlOrderOptions.Dispose ();
				ctrlOrderOptions = null;
			}

			if (homeView != null) {
				homeView.Dispose ();
				homeView = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}
		}
	}
}
