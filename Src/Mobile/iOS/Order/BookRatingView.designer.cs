// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookRatingView")]
	partial class BookRatingView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton submitRatingBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView ratingTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (submitRatingBtn != null) {
				submitRatingBtn.Dispose ();
				submitRatingBtn = null;
			}

			if (ratingTableView != null) {
				ratingTableView.Dispose ();
				ratingTableView = null;
			}
		}
	}
}
