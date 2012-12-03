// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BookStreetNumberView")]
	partial class BookStreetNumberView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblStreetNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtStreetNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblStreetName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.NavigateTextField txtNavigateSearch { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRefineAddress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
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

			if (txtNavigateSearch != null) {
				txtNavigateSearch.Dispose ();
				txtNavigateSearch = null;
			}

			if (lblRefineAddress != null) {
				lblRefineAddress.Dispose ();
				lblRefineAddress = null;
			}
		}
	}
}
