// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("HistoryTabView")]
	partial class HistoryTabView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblInfo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableHistory { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNoHistory { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblInfo != null) {
				lblInfo.Dispose ();
				lblInfo = null;
			}

			if (tableHistory != null) {
				tableHistory.Dispose ();
				tableHistory = null;
			}

			if (lblNoHistory != null) {
				lblNoHistory.Dispose ();
				lblNoHistory = null;
			}
		}
	}
}
