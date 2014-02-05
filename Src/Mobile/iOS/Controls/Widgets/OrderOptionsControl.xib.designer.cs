// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register ("OrderOptionsControl")]
	partial class OrderOptionsControl
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AddressTextBox viewDestination { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AddressTextBox viewPickup { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.VehicleTypeAndEstimateView viewVehicleType { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (viewDestination != null) {
				viewDestination.Dispose ();
				viewDestination = null;
			}

			if (viewPickup != null) {
				viewPickup.Dispose ();
				viewPickup = null;
			}

			if (viewVehicleType != null) {
				viewVehicleType.Dispose ();
				viewVehicleType = null;
			}
		}
	}
}
