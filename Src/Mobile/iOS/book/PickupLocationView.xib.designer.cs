// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("PickupLocationView")]
	partial class PickupLocationView
	{
		[Outlet]
		MonoTouch.UIKit.UITextField txtAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtAptSuite { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtTime { get; set; }

		[Outlet]
		MonoTouch.MapKit.MKMapView mapPickUp { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imageFieldBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPickup { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtAddress != null) {
				txtAddress.Dispose ();
				txtAddress = null;
			}

			if (txtAptSuite != null) {
				txtAptSuite.Dispose ();
				txtAptSuite = null;
			}

			if (txtRingCode != null) {
				txtRingCode.Dispose ();
				txtRingCode = null;
			}

			if (txtTime != null) {
				txtTime.Dispose ();
				txtTime = null;
			}

			if (mapPickUp != null) {
				mapPickUp.Dispose ();
				mapPickUp = null;
			}

			if (imageFieldBackground != null) {
				imageFieldBackground.Dispose ();
				imageFieldBackground = null;
			}

			if (tableAddress != null) {
				tableAddress.Dispose ();
				tableAddress = null;
			}

			if (lblDestination != null) {
				lblDestination.Dispose ();
				lblDestination = null;
			}

			if (lblPickup != null) {
				lblPickup.Dispose ();
				lblPickup = null;
			}
		}
	}
}
