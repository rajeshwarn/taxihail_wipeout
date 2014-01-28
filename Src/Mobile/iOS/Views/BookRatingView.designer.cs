// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookRatingView")]
	partial class BookRatingView
	{
		[Outlet]
		MonoTouch.UIKit.UITableView ratingTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ratingTableView != null) {
				ratingTableView.Dispose ();
				ratingTableView = null;
			}
		}
	}
}
