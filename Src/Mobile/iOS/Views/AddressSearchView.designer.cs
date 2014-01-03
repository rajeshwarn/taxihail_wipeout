// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("AddressSearchView")]
	partial class AddressSearchView
	{
		[Outlet]
		MonoTouch.UIKit.UITableView AddressListView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField SearchTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AddressListView != null) {
				AddressListView.Dispose ();
				AddressListView = null;
			}

			if (SearchTextField != null) {
				SearchTextField.Dispose ();
				SearchTextField = null;
			}
		}
	}
}
