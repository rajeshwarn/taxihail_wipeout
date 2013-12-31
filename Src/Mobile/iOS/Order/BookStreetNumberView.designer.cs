// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookStreetNumberView")]
	partial class BookStreetNumberView
	{
		[Outlet]
		GradientButton btnClear { get; set; }

		[Outlet]
		GradientButton btnPlaces { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStreetNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtStreetNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStreetName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRefineAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSearch { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnClear != null) {
				btnClear.Dispose ();
				btnClear = null;
			}

			if (btnPlaces != null) {
				btnPlaces.Dispose ();
				btnPlaces = null;
			}

			if (lblStreetNumber != null) {
				lblStreetNumber.Dispose ();
				lblStreetNumber = null;
			}

			if (txtStreetNumber != null) {
				txtStreetNumber.Dispose ();
				txtStreetNumber = null;
			}

			if (lblStreetName != null) {
				lblStreetName.Dispose ();
				lblStreetName = null;
			}

			if (lblRefineAddress != null) {
				lblRefineAddress.Dispose ();
				lblRefineAddress = null;
			}

			if (btnSearch != null) {
				btnSearch.Dispose ();
				btnSearch = null;
			}
		}
	}
}
