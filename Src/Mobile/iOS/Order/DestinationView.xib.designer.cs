// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("DestinationView")]
	partial class DestinationView
	{
		[Outlet]
		MonoTouch.MapKit.MKMapView mapDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imageFieldBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableSimilarAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDestination { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDistance { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPrice { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (mapDestination != null) {
				mapDestination.Dispose ();
				mapDestination = null;
			}

			if (txtAddress != null) {
				txtAddress.Dispose ();
				txtAddress = null;
			}

			if (imageFieldBackground != null) {
				imageFieldBackground.Dispose ();
				imageFieldBackground = null;
			}

			if (tableSimilarAddress != null) {
				tableSimilarAddress.Dispose ();
				tableSimilarAddress = null;
			}

			if (lblDestination != null) {
				lblDestination.Dispose ();
				lblDestination = null;
			}

			if (lblDistance != null) {
				lblDistance.Dispose ();
				lblDistance = null;
			}

			if (lblPrice != null) {
				lblPrice.Dispose ();
				lblPrice = null;
			}
		}
	}
}
