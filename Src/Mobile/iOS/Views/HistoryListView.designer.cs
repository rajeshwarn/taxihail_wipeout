// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register ("HistoryListView")]
    partial class HistoryListView
	{
		[Outlet]
		UIKit.UILabel lblNoHistory { get; set; }

		[Outlet]
		UIKit.UITableView tableOrders { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tableOrders != null) {
				tableOrders.Dispose ();
				tableOrders = null;
			}

			if (lblNoHistory != null) {
				lblNoHistory.Dispose ();
				lblNoHistory = null;
			}
		}
	}
}
