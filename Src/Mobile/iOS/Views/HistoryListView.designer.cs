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
    [Register ("HistoryListView")]
    partial class HistoryListView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblNoHistory { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableOrders { get; set; }
		
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
